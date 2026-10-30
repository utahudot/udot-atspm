using Utah.Udot.Atspm.Exceptions;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.Atspm.Business.TurningMovementCounts;

/// <summary>
/// Converts one configured device's count data into normalized movements for the shared TMC report.
/// Implementations own source-specific reading and interpretation; the device count-source adapter
/// handles device selection and combines their results. This is a report decoder, not a logging decoder.
/// </summary>
/// <remarks>
/// MinimumBinMinutes, CanDecode and DecodeAsync must be implemented. Other members have defaults
/// and only need overriding when the source requires different behavior.
/// </remarks>
public interface ITurningMovementCountDecoder
{
    /// <summary>
    /// Required. Positive base interval in minutes supported by this source.
    /// Requested report bins must be multiples of this value (for example, 15 for stored 15-minute counts).
    /// </summary>
    int MinimumBinMinutes { get; }

    /// <summary>Optional capability, default false. Supports unfiltered zone evidence for lane review.</summary>
    bool SupportsLaneReconciliation => false;

    /// <summary>
    /// Optional override. Singular noun used in report labels and duplicate-device warnings.
    /// Defaults to "device"; camera decoders use "camera". The adapter appends "s" for plural labels.
    /// </summary>
    string DeviceLabel => "device";

    /// <summary>
    /// Optional override. Validates source-specific connection or identity settings before data is read.
    /// The default does nothing. Throw ReportException for invalid configuration.
    /// </summary>
    /// <param name="device">Required configured device to validate.</param>
    void ValidateDevice(Device device) { }

    /// <summary>
    /// Optional override. Returns a stable identity used to collapse duplicate device selections before reading counts.
    /// Defaults to the configured device ID. Override to recognize multiple configurations of the same physical source.
    /// </summary>
    /// <param name="device">Required configured device whose source identity is being resolved.</param>
    /// <param name="token">Required cancellation argument; CancellationToken.None is allowed.</param>
    Task<string> IdentityAsync(Device device, CancellationToken token) => Task.FromResult($"device:{device.Id}");

    /// <summary>
    /// Optional override. Applies source-specific rules across all selected devices' decoded counts.
    /// The default retains every count. Override for rules such as duplicate-zone suppression.
    /// </summary>
    /// <param name="counts">Required combined counts; may be empty.</param>
    /// <param name="warnings">Required mutable collection to receive warnings about merging or excluded counts.</param>
    IReadOnlyList<MovementCount> MergeCounts(IEnumerable<MovementCount> counts, ICollection<string> warnings) => counts.ToList();

    /// <summary>
    /// Required implementation. Indicates whether this decoder supports the device's type and configuration.
    /// Returning false rejects the selection before decoding; it does not indicate whether the device is reachable.
    /// </summary>
    /// <param name="device">Required configured device to check.</param>
    bool CanDecode(Device device);

    /// <summary>
    /// Required implementation. Reads one device's source and returns normalized counts and warnings.
    /// Count timestamps must use local report time so the shared builder can place them into the requested bins.
    /// </summary>
    /// <param name="request">Required device, location, settings and report options.</param>
    /// <param name="cancellationToken">Required cancellation argument; CancellationToken.None is allowed.</param>
    Task<TmcDecodeResult> DecodeAsync(TmcDecodeRequest request, CancellationToken cancellationToken);
}

/// <summary>Input for decoding one device within a selected group of count sources.</summary>
/// <param name="Location">
/// Required location context. Groups devices and supplies report identity and coordinates for timezone resolution.
/// Vision statistics do not use its approach/detector configuration to interpret counts.
/// </param>
/// <param name="Device">Required device being read, including its connection and archive identity.</param>
/// <param name="Settings">
/// Required settings dictionary, normally the device properties; may be empty.
/// Individual settings are decoder-specific, such as the optional Vision TmcLayer setting.
/// </param>
/// <param name="Options">Required report options, including the local date range and requested bin size.</param>
public record TmcDecodeRequest(Location Location, Device Device,
    IReadOnlyDictionary<string, object> Settings, TurningMovementCountsOptions Options)
{
    /// <summary>
    /// Optional group context, defaulting to an empty list. The adapter supplies the selected devices
    /// after identity deduplication. Current Vision decoders do not consume this property.
    /// </summary>
    public IReadOnlyList<Device> Cameras { get; init; } = Array.Empty<Device>();
}

/// <summary>Normalized counts and diagnostic information returned by a single device decoder.</summary>
/// <param name="Counts">Required count collection; use an empty collection when no counts are available.</param>
/// <param name="Warnings">Required warning collection; use an empty collection when there are no warnings.</param>
public record TmcDecodeResult(IReadOnlyList<MovementCount> Counts, IReadOnlyList<string> Warnings)
{
    /// <summary>Optional zone evidence, before chart filtering or cross-zone aggregation.</summary>
    public IReadOnlyList<TmcZoneEvidence> ZoneEvidence { get; init; } = Array.Empty<TmcZoneEvidence>();
    /// <summary>
    /// Optional, defaults to true. Indicates that the source responded, even if it returned no counts.
    /// Set false when the source could not be read. If every selected device returns false, the adapter
    /// fails the report with HTTP 503. A true value does not guarantee complete data coverage.
    /// </summary>
    public bool Responded { get; init; } = true;
}

/// <summary>Source observations, independent of configured ATSPM lanes. Zero counts do not prove a prohibited movement.</summary>
public record TmcZoneEvidence(int DeviceId, string ZoneName, long Through, long Left, long Right, int Bins)
{
    /// <summary>Optional source zone ID; identifies unnamed zones without inventing a matching name.</summary>
    public long? ZoneId { get; init; }
}
