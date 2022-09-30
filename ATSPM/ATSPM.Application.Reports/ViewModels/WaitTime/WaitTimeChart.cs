using ATSPM.Application.Reports.ViewModels.YellowRedActivations;
using System;

namespace ATSPM.Application.Reports.ViewModels.WaitTime
{

    /// <summary>
    /// Wait Time chart
    /// </summary>
    public class WaitTimeChart
    {
        public string ChartName { get; set; }
        public string SignalId { get; set; }
        public string SignalLocation { get; set; }
        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string DetectionTypes { get; set; }
        public System.Collections.Generic.ICollection<YellowRedActivationsPlan> Plans { get; set; }
        public System.Collections.Generic.ICollection<GapOut> GapOuts { get; set; }
        public System.Collections.Generic.ICollection<MaxOut> MaxOuts { get; set; }
        public System.Collections.Generic.ICollection<ForceOff> ForceOffs { get; set; }
        public System.Collections.Generic.ICollection<Unknown> Unknowns { get; set; }
        public System.Collections.Generic.ICollection<Average> Average { get; set; }
    }
}