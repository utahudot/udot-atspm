using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.PedVsFailure
{
    /// <summary>
    /// Ped Vs Failure chart
    /// </summary>
    public class PedsVsFailureChart
    {
        public PedsVsFailureChart(string chartName, ICollection<CyclesWithPeds> cyclesWithPeds, ICollection<Failures> failures)
        {
            ChartName = chartName;
            CyclesWithPeds = cyclesWithPeds;
            Failures = failures;
        }

        public string ChartName { get; internal set; }
        public ICollection<CyclesWithPeds> CyclesWithPeds { get; internal set; }
        public ICollection<Failures> Failures { get; internal set; }
    }
}