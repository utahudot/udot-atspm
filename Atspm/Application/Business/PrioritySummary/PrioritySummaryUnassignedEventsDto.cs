using Utah.Udot.Atspm.Business.Common;

namespace Utah.Udot.Atspm.Business.PrioritySummary
{
    public sealed record PrioritySummaryUnassignedEventsDto(
        IReadOnlyList<DataPointForInt> EarlyGreen,
        IReadOnlyList<DataPointForInt> ExtendGreen
    );
}
