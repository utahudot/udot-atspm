using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.SplitMonitor;

/// <summary>
/// Split Monitor chart
/// </summary>
public class SplitMonitorResult:ApproachResult
{

    public SplitMonitorResult(
        int approachId,
        string signalId,
        DateTime start,
        DateTime end,
        int phaseNumber,
        ICollection<PlanSplitMonitorData> plans,
        ICollection<Split> programedSplits,
        ICollection<SplitMonitorGapOut> gapOuts,
        ICollection<SplitMonitorMaxOut> maxOuts,
        ICollection<SplitMonitorForceOff> forceOffs,
        ICollection<SplitMonitorUnknown> unknowns,
        ICollection<Peds> peds) : base(approachId, signalId, start, end)
    {
        PhaseNumber = phaseNumber;
        Plans = plans;
        ProgramedSplits = programedSplits;
        GapOuts = gapOuts;
        MaxOuts = maxOuts;
        ForceOffs = forceOffs;
        Unknowns = unknowns;
        Peds = peds;
    }
    public int PhaseNumber { get; internal set; }
    public ICollection<PlanSplitMonitorData> Plans { get; internal set; }
    public ICollection<Split> ProgramedSplits { get; internal set; }
    public ICollection<SplitMonitorGapOut> GapOuts { get; internal set; }
    public ICollection<SplitMonitorMaxOut> MaxOuts { get; internal set; }
    public ICollection<SplitMonitorForceOff> ForceOffs { get; internal set; }
    public ICollection<SplitMonitorUnknown> Unknowns { get; internal set; }
    public ICollection<Peds> Peds { get; internal set; }
}
