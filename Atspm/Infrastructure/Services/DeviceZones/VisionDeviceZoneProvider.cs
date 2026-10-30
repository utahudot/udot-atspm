using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Services;

namespace Utah.Udot.Atspm.Infrastructure.Services.DeviceZones;

public sealed class VisionDeviceZoneProvider(IHttpClientFactory clients) : IDeviceZoneProvider
{
    public bool CanRead(Device device) =>
        device.DeviceType is DeviceTypes.FIRCamera or DeviceTypes.AICamera &&
        string.Equals(device.DeviceConfiguration?.Product?.Manufacturer, "Econolite", StringComparison.OrdinalIgnoreCase) &&
        (device.DeviceConfiguration?.Product?.Model?.Contains("Vision", StringComparison.OrdinalIgnoreCase) ?? false);

    public async Task<IReadOnlyList<string>> ReadZonesAsync(Device device, CancellationToken cancellationToken)
    {
        var config = device.DeviceConfiguration;
        var path = config?.Path?.Trim().TrimEnd('/');
        var identifier = Regex.Replace(device.DeviceIdentifier ?? "", "-bins$", "");
        if (!IPAddress.TryParse(device.Ipaddress, out _) || config == null || config.Port is < 1 or > 65535 ||
            string.IsNullOrEmpty(identifier) || !identifier.All(char.IsDigit) ||
            string.IsNullOrWhiteSpace(path) || !path.StartsWith('/') || path.StartsWith("//") || path.IndexOfAny(['?', '#', '\\']) >= 0)
            throw new ArgumentException($"Device {device.Id}: invalid zone connection settings.");
        path = Regex.Replace(path, @"/(?:\d+|\[Device:DeviceIdentifier\])/(?:detections|bin-statistics)$", "", RegexOptions.IgnoreCase);
        var uri = new UriBuilder("http", device.Ipaddress, config.Port) { Path = $"{path}/{identifier}/zones" }.Uri;
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        if (!string.IsNullOrEmpty(config.UserName))
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.UserName}:{config.Password}")));
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        using var response = await clients.CreateClient("DeviceZones").SendAsync(request, timeout.Token);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
        // An invalid inventory must never look like an empty, successful configuration.
        return json.RootElement.GetProperty("zones").EnumerateArray().Select(zone =>
            zone.GetProperty("name").GetString() ?? throw new InvalidDataException("Unnamed device zone; verify device configuration.")).ToArray();
    }
}
