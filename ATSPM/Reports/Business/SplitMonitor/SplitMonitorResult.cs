using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.SplitMonitor;

/// <summary>
/// Split Monitor chart
/// </summary>
public class SplitMonitorResult:SignalResult
{
    public SplitMonitorResult(int phaseNumber, string signalId, DateTime start, DateTime end): base(signalId, start, end)
    {
        PhaseNumber = phaseNumber;
        Plans = new List<PlanSplitMonitorData>();
        ProgramedSplits = new List<Split>();
        GapOuts = new List<SplitMonitorEvent>();
        MaxOuts = new List<SplitMonitorEvent>();
        ForceOffs = new List<SplitMonitorEvent>();
        Unknowns = new List<SplitMonitorEvent>();
        Peds = new List<SplitMonitorEvent>();
    }

    public SplitMonitorResult(
        string signalId,
        DateTime start,
        DateTime end,
        int phaseNumber,
        ICollection<PlanSplitMonitorData> plans,
        ICollection<Split> programedSplits,
        ICollection<SplitMonitorEvent> gapOuts,
        ICollection<SplitMonitorEvent> maxOuts,
        ICollection<SplitMonitorEvent> forceOffs,
        ICollection<SplitMonitorEvent> unknowns,
        ICollection<SplitMonitorEvent> peds) : base(signalId, start, end)
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
    public ICollection<SplitMonitorEvent> GapOuts { get; internal set; }
    public ICollection<SplitMonitorEvent> MaxOuts { get; internal set; }
    public ICollection<SplitMonitorEvent> ForceOffs { get; internal set; }
    public ICollection<SplitMonitorEvent> Unknowns { get; internal set; }
    public ICollection<SplitMonitorEvent> Peds { get; internal set; }
}