using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.YellowRedActivations
{
    /// <summary>
    /// Yellow Red Activations chart
    /// </summary>
    public class YellowRedActivationsResult : ApproachResult
    {
        public YellowRedActivationsResult(
            string locationId,
            int approachId,
            string direction,
            string movementType,
            int protectedPhaseNumber,
            int? permissivePhaseNumber,
            bool isPermissivePhase,
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
            ICollection<DataPointForDouble> detectorEvents) : base(approachId, locationId, start, end)
        {
            ApproachId = approachId;
            Direction = direction;
            MovementType = movementType;
            ProtectedPhaseNumber = protectedPhaseNumber;
            PermissivePhaseNumber = permissivePhaseNumber;
            IsPermissivePhase = isPermissivePhase;
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

        public string Direction { get; set; }
        public string MovementType { get; set; }
        public int ProtectedPhaseNumber { get; internal set; }
        public int? PermissivePhaseNumber { get; internal set; }
        public bool IsPermissivePhase { get; }
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