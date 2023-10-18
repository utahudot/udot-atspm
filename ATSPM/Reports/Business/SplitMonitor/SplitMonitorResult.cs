using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.SplitMonitor;

/// <summary>
/// Split Monitor chart
/// </summary>
public class SplitMonitorResult : SignalResult
{
    public SplitMonitorResult(int phaseNumber, string phaseDescription, string signalId, DateTime start, DateTime end) : base(signalId, start, end)
    {
        PhaseNumber = phaseNumber;
        PhaseDescription = phaseDescription;
        Plans = new List<PlanSplitMonitorDTO>();
        ProgrammedSplits = new List<DataPointForDouble>();
        GapOuts = new List<DataPointForDouble>();
        MaxOuts = new List<DataPointForDouble>();
        ForceOffs = new List<DataPointForDouble>();
        Unknowns = new List<DataPointForDouble>();
        Peds = new List<DataPointForDouble>();
    }

    public SplitMonitorResult(
        string signalId,
        DateTime start,
        DateTime end,
        int phaseNumber,
        ICollection<PlanSplitMonitorDTO> plans,
        ICollection<DataPointForDouble> programmedSplits,
        ICollection<DataPointForDouble> gapOuts,
        ICollection<DataPointForDouble> maxOuts,
        ICollection<DataPointForDouble> forceOffs,
        ICollection<DataPointForDouble> unknowns,
        ICollection<DataPointForDouble> peds) : base(signalId, start, end)
    {
        PhaseNumber = phaseNumber;
        Plans = plans;
        ProgrammedSplits = programmedSplits;
        GapOuts = gapOuts;
        MaxOuts = maxOuts;
        ForceOffs = forceOffs;
        Unknowns = unknowns;
        Peds = peds;
    }
    public int PhaseNumber { get; internal set; }

    public string PhaseDescription { get; internal set; }
    public ICollection<PlanSplitMonitorDTO> Plans { get; internal set; }
    public ICollection<DataPointForDouble> ProgrammedSplits { get; internal set; }
    public ICollection<DataPointForDouble> GapOuts { get; internal set; }
    public ICollection<DataPointForDouble> MaxOuts { get; internal set; }
    public ICollection<DataPointForDouble> ForceOffs { get; internal set; }
    public ICollection<DataPointForDouble> Unknowns { get; internal set; }
    public ICollection<DataPointForDouble> Peds { get; internal set; }
}
