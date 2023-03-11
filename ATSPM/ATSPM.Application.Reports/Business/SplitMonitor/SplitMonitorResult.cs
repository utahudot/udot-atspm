using ATSPM.Application.Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.SplitMonitor;

/// <summary>
/// Split Monitor chart
/// </summary>
public class SplitMonitorResult
{    

    public string ChartName { get; internal set; }
    public string SignalId { get; internal set; }
    public string SignalLocation { get; internal set; }
    public int PhaseNumber { get; internal set; }
    public string PhaseDescription { get; internal set; }
    public DateTime Start { get; internal set; }
    public DateTime End { get; internal set; }
    public ICollection<PlanSplitMonitorData> Plans { get; internal set; }
    public ICollection<Split> ProgramedSplits { get; internal set; }
    public ICollection<SplitMonitorGapOut> GapOuts { get; internal set; }
    public ICollection<SplitMonitorMaxOut> MaxOuts { get; internal set; }
    public ICollection<SplitMonitorForceOff> ForceOffs { get; internal set; }
    public ICollection<SplitMonitorUnknown> Unknowns { get; internal set; }
    public ICollection<Peds> Peds { get; internal set; }
}
