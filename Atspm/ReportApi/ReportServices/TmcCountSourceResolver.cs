using Utah.Udot.Atspm.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utah.Udot.Atspm.Business.TurningMovementCounts;

namespace Utah.Udot.Atspm.ReportApi.ReportServices;

/// <summary>Resolves configured sources while preserving the existing atspm/devices request contract.</summary>
public sealed class TmcCountSourceResolver(IDeviceRepository devices, IServiceProvider services, IConfiguration configuration)
{
    public (ITurningMovementCountSource Source, TmcCountSourceRequest Request) Resolve(Location location, TurningMovementCountsOptions options)
    {
        var key = options.Source ?? "atspm";
        // Independent, default-off gate: lane review must not be enabled just by enabling TMC sources.
        if (options.ReconcileLanes && (!configuration.GetValue<bool>("Features:LaneReconciliation") || key != "devices"))
            throw new ReportException(400, "Lane reconciliation is disabled or no device source was selected.");
        var request = new TmcCountSourceRequest(location, options);
        if (key == "devices")
        {
            if (!options.ReconcileLanes && !configuration.GetValue<bool>("Features:TmcDeviceSources")) throw new ReportException(400, "Device TMC sources are disabled.");
            var ids = options.DeviceIds?.Distinct().ToArray() ?? Array.Empty<int>();
            if (ids.Length == 0) throw new ReportException(400, "Select at least one device.");
            var selected = devices.GetList().Where(d => ids.Contains(d.Id)).OrderBy(d => d.Id).ToList();
            if (selected.Count != ids.Length || selected.Any(d => d.LocationId != location.Id))
                throw new ReportException(400, "Every requested device must belong to the requested location version.");
            // A device may expose several report decoders using the existing string property.
            // Infer only an unambiguous common decoder so old single-decoder requests still work.
            var configured = selected.Select(d =>
                (d.DeviceProperties != null && d.DeviceProperties.TryGetValue("TmcDecoder", out var value)
                    ? value?.ToString() : null)?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.Ordinal).ToArray() ?? Array.Empty<string>()).ToArray();
            var common = configured.Skip(1).Aggregate(configured[0].AsEnumerable(),
                (names, next) => names.Intersect(next, StringComparer.Ordinal)).ToArray();
            key = options.Decoder?.Trim();
            if (string.IsNullOrEmpty(key))
            {
                if (common.Length != 1) throw new ReportException(400, "Select a decoder configured on every requested device.");
                key = common[0];
            }
            if (!common.Contains(key, StringComparer.Ordinal))
                throw new ReportException(400, "The selected decoder must be configured on every requested device.");
            request = request with { Devices = selected };
        }
        var source = services.GetKeyedService<ITurningMovementCountSource>(key)
            ?? throw new ReportException(400, $"Unknown count source '{key}'.");
        // Directly naming a device source must not bypass its feature gate or validated device selection.
        if (source is IDeviceCountSource && (options.Source != "devices" || request.Devices.Count == 0))
            throw new ReportException(400, "Device sources require source=devices and configured device IDs.");
        return (source, request);
    }
}
