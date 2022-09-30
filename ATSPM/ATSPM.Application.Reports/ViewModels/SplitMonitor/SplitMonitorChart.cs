using ATSPM.Application.Reports.ViewModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.SplitMonitor;

/// <summary>
/// Split Monitor chart
/// </summary>
public class SplitMonitorChart
{
    public string ChartName { get; set; }
    public string SignalId { get; set; }
    public string SignalLocation { get; set; }
    public int PhaseNumber { get; set; }
    public string PhaseDescription { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public ICollection<Plan> Plans { get; set; }
    public ICollection<Split> Splits { get; set; }
    public ICollection<GapOut> GapOuts { get; set; }
    public ICollection<MaxOut> MaxOuts { get; set; }
    public ICollection<ForceOff> ForceOffs { get; set; }
    public ICollection<Unknown> Unknowns { get; set; }
    public ICollection<Peds> Peds { get; set; }
}
