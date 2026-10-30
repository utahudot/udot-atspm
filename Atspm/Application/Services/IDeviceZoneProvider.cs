using Utah.Udot.Atspm.Data.Models;

namespace Utah.Udot.Atspm.Services;

/// <summary>Reads device configuration for Get Zones. Count evidence belongs to report decoders.</summary>
public interface IDeviceZoneProvider
{
    /// <summary>Whether this provider understands the configured device's hardware.</summary>
    bool CanRead(Device device);

    /// <summary>Returns the complete zone-name inventory, or throws if it cannot be read.</summary>
    Task<IReadOnlyList<string>> ReadZonesAsync(Device device, CancellationToken cancellationToken);
}
