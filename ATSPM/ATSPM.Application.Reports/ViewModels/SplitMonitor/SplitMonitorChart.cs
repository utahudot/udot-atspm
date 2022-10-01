using ATSPM.Application.Reports.ViewModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.SplitMonitor;

/// <summary>
/// Split Monitor chart
/// </summary>
public class SplitMonitorChart
{
    public string ChartName { get; internal set; }
    public string SignalId { get; internal set; }
    public string SignalLocation { get; internal set; }
    public int PhaseNumber { get; internal set; }
    public string PhaseDescription { get; internal set; }
    public DateTime Start { get; internal set; }
    public DateTime End { get; internal set; }
    public ICollection<Plan> Plans { get; internal set; }
    public ICollection<Split> Splits { get; internal set; }
    public ICollection<GapOut> GapOuts { get; internal set; }
    public ICollection<MaxOut> MaxOuts { get; internal set; }
    public ICollection<ForceOff> ForceOffs { get; internal set; }
    public ICollection<Unknown> Unknowns { get; internal set; }
    public ICollection<Peds> Peds { get; internal set; }
}
