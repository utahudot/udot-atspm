using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.PhaseTermination;

/// <summary>
/// Phase Termination chart
/// </summary>
public class PhaseTerminationResult : LocationResult
{
    public PhaseTerminationResult(
        string locationId,
        DateTime start,
        DateTime end,
        int consecutiveCount,
        ICollection<Plan> plans,
        ICollection<Phase> phases) : base(locationId, start, end)
    {
        ConsecutiveCount = consecutiveCount;
        Plans = plans;
        Phases = phases;
    }
    public int ConsecutiveCount { get; internal set; }
    public ICollection<Plan> Plans { get; internal set; }
    public ICollection<Phase> Phases { get; internal set; }

}
