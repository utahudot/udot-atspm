using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PhaseTermination;

/// <summary>
/// Phase Termination chart
/// </summary>
public class PhaseTerminationResult:SignalResult
{
    public PhaseTerminationResult(
        string signalId,
        DateTime start,
        DateTime end,
        int consecutiveCount,
        ICollection<Plan> plans,
        ICollection<GapOut> gapOuts,
        ICollection<MaxOut> maxOuts,
        ICollection<ForceOff> forceOffs,
        ICollection<PedWalkBegin> pedWalkBegins,
        ICollection<UnknownTermination> unknownTerminations):base(signalId,  start, end)
    {
        ConsecutiveCount = consecutiveCount;
        Plans = plans;
        GapOuts = gapOuts;
        MaxOuts = maxOuts;
        ForceOffs = forceOffs;
        PedWalkBegins = pedWalkBegins;
        UnknownTerminations = unknownTerminations;
    }
    public int ConsecutiveCount { get; internal set; }
    public ICollection<Plan> Plans { get; internal set; }
    public ICollection<GapOut> GapOuts { get; internal set; }
    public ICollection<MaxOut> MaxOuts { get; internal set; }
    public ICollection<ForceOff> ForceOffs { get; internal set; }
    public ICollection<PedWalkBegin> PedWalkBegins { get; internal set; }
    public ICollection<UnknownTermination> UnknownTerminations { get; internal set; }
}
