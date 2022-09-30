using ATSPM.Application.Reports.ViewModels;
using System;

namespace ATSPM.Application.Reports.ViewModels.YellowRedActivations
{
    /// <summary>
    /// Yellow Red Activations chart
    /// </summary>
    public class YellowRedActivationsChart
    {
        public string ChartName { get; set; }
        public string SignalId { get; set; }
        public string SignalLocation { get; set; }
        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string TitleMessage { get; set; }
        public int TotalViolations { get; set; }
        public int SevereViolations { get; set; }
        public int YellowLightOccurences { get; set; }
        public string StatisticsMessage { get; set; }
        public System.Collections.Generic.ICollection<YellowRedActivationsPlan> Plans { get; set; }
        public System.Collections.Generic.ICollection<RedEvents> RedEvents { get; set; }
        public System.Collections.Generic.ICollection<YellowEvents> YellowEvents { get; set; }
        public System.Collections.Generic.ICollection<RedClearanceEvents> RedClearanceEvents { get; set; }
        public System.Collections.Generic.ICollection<DetectorEvents> DetectorEvents { get; set; }
    }
}