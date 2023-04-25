using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.YellowRedActivations
{
    /// <summary>
    /// Yellow Red Activations chart
    /// </summary>
    public class YellowRedActivationsResult
    {
        public YellowRedActivationsResult(string chartName,
            int approachId,
            string approachDescription,
            int phaseNumber,
            DateTime start,
            DateTime end,
            int totalViolations,
            int severeViolations,
            int yellowLightOccurences,
            ICollection<YellowRedActivationsPlan> plans,
            ICollection<YellowRedActivationEvent> redEvents,
            ICollection<YellowRedActivationEvent> yellowEvents,
            ICollection<YellowRedActivationEvent> redClearanceEvents,
            ICollection<YellowRedActivationEvent> detectorEvents)
        {
            ChartName = chartName;
            ApproachId = approachId;
            ApproachDescription = approachDescription;
            PhaseNumber = phaseNumber;
            Start = start;
            End = end;
            TotalViolations = totalViolations;
            SevereViolations = severeViolations;
            YellowLightOccurences = yellowLightOccurences;
            Plans = plans;
            RedEvents = redEvents;
            YellowEvents = yellowEvents;
            RedClearanceEvents = redClearanceEvents;
            DetectorEvents = detectorEvents;
        }

        public string ChartName { get; internal set; }
        public int ApproachId { get; internal set; }
        public string ApproachDescription { get; internal set; }
        public int PhaseNumber { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public int TotalViolations { get; internal set; }
        public int SevereViolations { get; internal set; }
        public int YellowLightOccurences { get; internal set; }
        public ICollection<YellowRedActivationsPlan> Plans { get; internal set; }
        public ICollection<YellowRedActivationEvent> RedEvents { get; internal set; }
        public ICollection<YellowRedActivationEvent> YellowEvents { get; internal set; }
        public ICollection<YellowRedActivationEvent> RedClearanceEvents { get; internal set; }
        public ICollection<YellowRedActivationEvent> DetectorEvents { get; internal set; }
    }
}