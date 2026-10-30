using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.Atspm.Business.TurningMovementCounts;

/// <summary>A report source supplies normalized counts; the report pipeline owns presentation and metrics.</summary>
public interface ITurningMovementCountSource
{
    Task<TmcCountSourceResult> ReadAsync(TmcCountSourceRequest request, CancellationToken cancellationToken);
}

// Marker for sources that require configuration-backed device selection.
public interface IDeviceCountSource : ITurningMovementCountSource { }

public record TmcCountSourceRequest(Location Location, TurningMovementCountsOptions Options)
{
    public IReadOnlyList<Device> Devices { get; init; } = Array.Empty<Device>();
}

public record TmcCountSourceResult(string Label, IReadOnlyList<MovementCount> Counts,
    IReadOnlyList<string> Warnings)
{
    public IReadOnlyList<Plan> Plans { get; init; } = Array.Empty<Plan>();
    // A source with discrete events may bin from the selected start; fixed camera bins remain aligned.
    public DateTime? BinOrigin { get; init; }
    public IReadOnlyList<TmcZoneEvidence> ZoneEvidence { get; init; } = Array.Empty<TmcZoneEvidence>();
    /// <summary>Selected device IDs after the decoder resolves aliases, including devices without evidence.</summary>
    public IReadOnlyList<int> SourceDeviceIds { get; init; } = Array.Empty<int>();
}
