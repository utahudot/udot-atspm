using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.YellowRedActivations
{
    /// <summary>
    /// Yellow Red Activations chart
    /// </summary>
    public class YellowRedActivationsResult:ApproachResult
    {
        public YellowRedActivationsResult(
            string signalId,
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
            ICollection<YellowRedActivationEvent> detectorEvents):base(approachId, signalId,  start, end)
        {
            ApproachId = approachId;
            ApproachDescription = approachDescription;
            PhaseNumber = phaseNumber;
            TotalViolations = totalViolations;
            SevereViolations = severeViolations;
            YellowLightOccurences = yellowLightOccurences;
            Plans = plans;
            RedEvents = redEvents;
            YellowEvents = yellowEvents;
            RedClearanceEvents = redClearanceEvents;
            DetectorEvents = detectorEvents;
        }

        public string ApproachDescription { get; internal set; }
        public int PhaseNumber { get; internal set; }
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