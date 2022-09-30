using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.GapVsDemand;

/// <summary>
/// Gap Vs Demand  chart
/// </summary>
public class GapVsDemandChart   
{
    public GapVsDemandChart(string chartName, ICollection<CyclesWithPeds> cyclesWithPeds, ICollection<Failures> failures)
    {
        ChartName = chartName;
        CyclesWithPeds = cyclesWithPeds;
        Failures = failures;
    }

    public string ChartName { get; internal set; }
    public ICollection<CyclesWithPeds> CyclesWithPeds { get; internal set; }
    public ICollection<Failures> Failures { get; internal set; }       

}
