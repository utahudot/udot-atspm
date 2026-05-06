namespace Utah.Udot.Atspm.Business.PrioritySummary
{
    public sealed record PrioritySummaryUnassignedEventsDto(
        IReadOnlyList<DateTime> EarlyGreen,
        IReadOnlyList<DateTime> ExtendGreen
    );
}
