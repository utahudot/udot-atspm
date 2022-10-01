using ATSPM.Application.Reports.ViewModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.YellowRedActivations
{
    /// <summary>
    /// Yellow Red Activations chart
    /// </summary>
    public class YellowRedActivationsChart
    {
        public YellowRedActivationsChart(string chartName,
            string signalId,
            string signalLocation,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            string titleMessage,
            int totalViolations,
            int severeViolations,
            int yellowLightOccurences,
            string statisticsMessage,
            ICollection<YellowRedActivationsPlan> plans,
            ICollection<RedEvents> redEvents,
            ICollection<YellowEvents> yellowEvents,
            ICollection<RedClearanceEvents> redClearanceEvents,
            ICollection<DetectorEvents> detectorEvents)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            Start = start;
            End = end;
            TitleMessage = titleMessage;
            TotalViolations = totalViolations;
            SevereViolations = severeViolations;
            YellowLightOccurences = yellowLightOccurences;
            StatisticsMessage = statisticsMessage;
            Plans = plans;
            RedEvents = redEvents;
            YellowEvents = yellowEvents;
            RedClearanceEvents = redClearanceEvents;
            DetectorEvents = detectorEvents;
        }

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public string SignalLocation { get; internal set; }
        public int PhaseNumber { get; internal set; }
        public string PhaseDescription { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public string TitleMessage { get; internal set; }
        public int TotalViolations { get; internal set; }
        public int SevereViolations { get; internal set; }
        public int YellowLightOccurences { get; internal set; }
        public string StatisticsMessage { get; internal set; }
        public System.Collections.Generic.ICollection<YellowRedActivationsPlan> Plans { get; internal set; }
        public System.Collections.Generic.ICollection<RedEvents> RedEvents { get; internal set; }
        public System.Collections.Generic.ICollection<YellowEvents> YellowEvents { get; internal set; }
        public System.Collections.Generic.ICollection<RedClearanceEvents> RedClearanceEvents { get; internal set; }
        public System.Collections.Generic.ICollection<DetectorEvents> DetectorEvents { get; internal set; }
    }
}