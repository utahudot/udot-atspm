using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.YellowRedActivations
{
    /// <summary>
    /// Yellow Red Activations chart
    /// </summary>
    public class YellowRedActivationsResult : ApproachResult
    {
        public YellowRedActivationsResult(
            string signalId,
            int approachId,
            string approachDescription,
            int phaseNumber,
            string phaseType,
            DateTime start,
            DateTime end,
            int totalViolations,
            int severeViolations,
            int yellowLightOccurences,
            ICollection<YellowRedActivationsPlan> plans,
            ICollection<DataPointForDouble> redEvents,
            ICollection<DataPointForDouble> yellowEvents,
            ICollection<DataPointForDouble> redClearanceEvents,
            ICollection<DataPointForDouble> detectorEvents) : base(approachId, signalId, start, end)
        {
            ApproachId = approachId;
            ApproachDescription = approachDescription;
            PhaseNumber = phaseNumber;
            PhaseType = phaseType;
            TotalViolations = totalViolations;
            SevereViolations = severeViolations;
            YellowLightOccurences = yellowLightOccurences;
            Plans = plans;
            RedEvents = redEvents;
            YellowEvents = yellowEvents;
            RedClearanceEvents = redClearanceEvents;
            DetectorEvents = detectorEvents;
        }

        public string ApproachDescription { get; set;  }
        public int PhaseNumber { get; internal set; }
        public string PhaseType { get; internal set; }
        public int TotalViolations { get; internal set; }
        public int SevereViolations { get; internal set; }
        public int YellowLightOccurences { get; internal set; }
        public ICollection<YellowRedActivationsPlan> Plans { get; internal set; }
        public ICollection<DataPointForDouble> RedEvents { get; internal set; }
        public ICollection<DataPointForDouble> YellowEvents { get; internal set; }
        public ICollection<DataPointForDouble> RedClearanceEvents { get; internal set; }
        public ICollection<DataPointForDouble> DetectorEvents { get; internal set; }
    }
}