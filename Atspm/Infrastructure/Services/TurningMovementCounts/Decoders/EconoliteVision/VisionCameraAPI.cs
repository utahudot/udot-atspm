using Utah.Udot.Atspm.Exceptions;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.ATSPM.Infrastructure.Services.EventLogDecoders;

namespace Utah.Udot.Atspm.Infrastructure.Services.TurningMovementCounts.Decoders.EconoliteVision;

// Shared across requests: the limit is per comm manager, not per report or camera.
public sealed class TmcCameraConcurrency
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> gates = new();
    public SemaphoreSlim For(string manager) => gates.GetOrAdd(manager, _ => new SemaphoreSlim(4));
}

public sealed class VisionCameraAPI(IHttpClientFactory clientFactory, TmcCameraConcurrency concurrency) : ITurningMovementCountDecoder
{
    private readonly HttpClient client = clientFactory.CreateClient("TmcCamera");
    private readonly ConcurrentDictionary<string, Task<JObject>> discovery = new();
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
    public string DeviceLabel => "camera";
    public int MinimumBinMinutes => 1;
    public bool SupportsLaneReconciliation => true;
    public void ValidateDevice(Device device)
    {
        _ = CameraIndex(device);
        _ = CamerasPath(device);
    }

    // Reuse the existing download path. Configurations may store either the camera collection
    // or a complete per-camera logging endpoint; reporting supplies its own camera and query range.
    private static string CamerasPath(Device device)
    {
        var path = device.DeviceConfiguration?.Path?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith('/') || path.StartsWith("//")
            || path.IndexOfAny(new[] { '?', '#', '\\' }) >= 0)
            throw new ReportException(400, $"Device {device.Id}: configure a camera API path in DeviceConfiguration.Path.");
        path = System.Text.RegularExpressions.Regex.Replace(path,
            @"/(?:\d+|\[Device:DeviceIdentifier\])/(?:detections|bin-statistics)$", "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (string.IsNullOrWhiteSpace(path))
            throw new ReportException(400, $"Device {device.Id}: the camera API collection path is missing.");
        return path;
    }
    public IReadOnlyList<MovementCount> MergeCounts(IEnumerable<MovementCount> counts, ICollection<string> warnings)
        => VisionCountNormalizer.Combine(counts, warnings);
    public static string CameraIndex(Device device) => RegexIndex(device.DeviceIdentifier);
    private static string RegexIndex(string value)
    {
        var index = (value ?? "").EndsWith("-bins", StringComparison.Ordinal) ? value[..^5] : value;
        if (string.IsNullOrWhiteSpace(index) || !index.All(char.IsDigit))
            throw new ReportException(400, $"Invalid Vision camera index '{value}'.");
        return index;
    }
    public static string Manager(Device device) => $"{device.Ipaddress}:{device.DeviceConfiguration?.Port}";

    public static bool IsVisionCamera(Device device)
    {
        var product = device.DeviceConfiguration?.Product;
        return device.DeviceStatus == DeviceStatus.Active && device.DeviceType is DeviceTypes.FIRCamera or DeviceTypes.AICamera
            && string.Equals(product?.Manufacturer, "Econolite", StringComparison.OrdinalIgnoreCase)
            && (product?.Model?.Contains("Vision", StringComparison.OrdinalIgnoreCase) ?? false);
    }
    public bool CanDecode(Device device) => IsVisionCamera(device)
        && IPAddress.TryParse(device.Ipaddress, out _)
        && device.DeviceConfiguration.Port is > 0 and <= 65535;

    // Canonicalize index/serial aliases so separately configured logging streams are never added twice.
    public async Task<string> IdentityAsync(Device device, CancellationToken token)
    {
        var index = CameraIndex(device);
        try
        {
            var path = CamerasPath(device);
            var listing = await discovery.GetOrAdd($"{Manager(device)}{path}", _ => ReadObject(device, path, token));
            var camera = listing["cameras"]?.FirstOrDefault(c => c["index"]?.ToString() == index || c["deviceId"]?.ToString() == index);
            index = camera?["index"]?.ToString() ?? index;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException) { token.ThrowIfCancellationRequested(); }
        return $"{Manager(device)}/{index}";
    }

    public async Task<TmcDecodeResult> DecodeAsync(TmcDecodeRequest request, CancellationToken cancellationToken)
    {
        var device = request.Device;
        if (!CanDecode(device)) throw new ReportException(400, $"Device {device.Id} cannot use {nameof(VisionCameraAPI)}.");
        ValidateDevice(device);
        if (request.Options.BinSize < 1)
            throw new ReportException(400, $"Device {device.Id}: bin size must be a multiple of 1 minutes.");
        var zone = TimeZoneInfo.FindSystemTimeZoneById(GeoTimeZone.TimeZoneLookup.GetTimeZone(request.Location.Latitude, request.Location.Longitude).Result);
        var warnings = new List<string>();
        var rows = new List<VisionCameraStatisticsEvent>();
        var responded = false;
        {
            var localStart = DateTime.SpecifyKind(request.Options.Start, DateTimeKind.Unspecified);
            var minutes = request.Options.BinSize;
            var aligned = localStart.Date.AddMinutes(Math.Floor(localStart.TimeOfDay.TotalMinutes / minutes) * minutes);
            var start = TimeZoneInfo.ConvertTimeToUtc(aligned, zone);
            var end = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(request.Options.End, DateTimeKind.Unspecified), zone);
            for (var day = start; day < end; day = day.AddDays(1))
            {
                var stop = day.AddDays(1) < end ? day.AddDays(1) : end;
                try { rows.AddRange(await ReadBins(device, day, stop, minutes, cancellationToken)); responded = true; }
                catch (ReportException) { throw; }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidDataException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var reason = ex is TaskCanceledException ? $"timed out after {RequestTimeout.TotalSeconds:0} seconds" : ex.Message.TrimEnd('.');
                    warnings.Add($"Camera {device.DeviceIdentifier}: no data for {day:yyyy-MM-dd} ({reason}).");
                    // An unreachable camera would make every remaining day wait out the same failure.
                    if (stop < end && (ex is TaskCanceledException
                        || ex is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized }
                        || ex is HttpRequestException { HttpRequestError: HttpRequestError.ConnectionError or HttpRequestError.NameResolutionError }))
                    {
                        var last = stop;
                        while (last.AddDays(1) < end) last = last.AddDays(1);
                        var failure = ex is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized } ? "login rejected" : "unreachable";
                        warnings.Add($"Camera {device.DeviceIdentifier}: {failure}; skipped {stop:yyyy-MM-dd}{(last > stop ? $" to {last:yyyy-MM-dd}" : "")}.");
                        break;
                    }
                }
            }
        }
        if (rows.Count == 0) warnings.Add($"Camera {device.DeviceIdentifier}: no bins in the requested range.");
        if (request.Options.ReconcileLanes)
        {
            // Reconciliation must see every zone, including zones absent from the location configuration.
            // Chart layer/name conventions and closest-advance filtering would hide the very errors being reviewed.
            var evidence = VisionZoneMapper.InRequestedRange(rows, request, zone, false)
                .GroupBy(r => (r.ZoneId, r.ZoneName, r.Timestamp)).Select(g => g.Last()).ToArray();
            if (evidence.Any(r => r.ThroughCount < 0 || r.LeftTurnCount < 0 || r.RightTurnCount < 0))
                throw new ReportException(502, $"Camera {device.DeviceIdentifier} (device {device.Id}): negative movement counts.");
            foreach (var duplicate in evidence.Where(r => !string.IsNullOrWhiteSpace(r.ZoneName)).GroupBy(r => r.ZoneName).Where(g => g.Select(r => r.ZoneId).Distinct().Count() > 1))
                warnings.Add($"Camera {device.DeviceIdentifier}: multiple zone IDs use the name {duplicate.Key}; verify configuration changes before editing lanes.");
            return new TmcDecodeResult(Array.Empty<MovementCount>(), warnings) {
                Responded = responded,
                ZoneEvidence = evidence.GroupBy(r => (r.ZoneId, r.ZoneName)).Select(g => new TmcZoneEvidence(device.Id, g.Key.ZoneName,
                    g.Sum(r => (long)r.ThroughCount), g.Sum(r => (long)r.LeftTurnCount),
                    g.Sum(r => (long)r.RightTurnCount), g.Select(r => r.Timestamp).Distinct().Count()) { ZoneId = g.Key.ZoneId }).ToArray()
            };
        }
        var mapped = VisionZoneMapper.Map(request, rows, request.Options.BinSize, zone, false);
        return mapped with { Warnings = warnings.Concat(mapped.Warnings).Distinct().ToArray(), Responded = responded };
    }

    private async Task<IReadOnlyList<VisionCameraStatisticsEvent>> ReadBins(Device device, DateTime start, DateTime end, int interval, CancellationToken token, bool retry = true)
    {
        var path = $"{CamerasPath(device)}/{CameraIndex(device)}/bin-statistics?start-time={start.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}&end-time={end.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}&interval={interval}";
        try
        {
            var body = await Read(device, path, token);
            var json = JObject.Parse(body);
            if (json["statistics"] is not JArray) throw new JsonException("Missing statistics array.");
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
            return new JsonToVisionCameraStatisticEventDecoder().Decode(device, stream, token).ToList();
        }
        catch (JsonException) when (retry && (end - start).TotalMinutes > interval)
        {
            var half = start.AddMinutes(Math.Max(interval, Math.Floor((end - start).TotalMinutes / interval / 2) * interval));
            var first = await ReadBins(device, start, half, interval, token, false);
            var second = await ReadBins(device, half, end, interval, token, false);
            return first.Concat(second).ToList();
        }
    }
    private async Task<JObject> ReadObject(Device device, string path, CancellationToken token) => JObject.Parse(await Read(device, path, token));
    private async Task<string> Read(Device device, string path, CancellationToken token)
    {
        var gate = concurrency.For(Manager(device));
        await gate.WaitAsync(token);
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeout.CancelAfter(RequestTimeout);
            var uri = new UriBuilder("http", device.Ipaddress, device.DeviceConfiguration.Port) { Path = path.Split('?')[0], Query = path.Contains('?') ? path.Split('?')[1] : "" }.Uri;
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            // Comm managers with login turned on accept the device configuration's credentials as HTTP Basic.
            var user = device.DeviceConfiguration.UserName;
            if (!string.IsNullOrEmpty(user))
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{device.DeviceConfiguration.Password}")));
            using var response = await client.SendAsync(request, timeout.Token);
            var text = await response.Content.ReadAsStringAsync(timeout.Token);
            // A rejection is that camera's warning; the other cameras' counts still report.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new HttpRequestException("login rejected; check the username and password on the device configuration", null, response.StatusCode);
            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new HttpRequestException($"rejected the request: {text[..Math.Min(text.Length, 200)]}", null, response.StatusCode);
            response.EnsureSuccessStatusCode();
            return text;
        }
        finally { gate.Release(); }
    }
}
