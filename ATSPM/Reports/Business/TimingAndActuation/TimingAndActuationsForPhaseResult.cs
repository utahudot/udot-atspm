using ATSPM.Data.Models;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.TimingAndActuation
{
    public class TimingAndActuationsForPhaseResult:ApproachResult
    {
        public TimingAndActuationsForPhaseResult(
            int approachId,
            string signalId,
            DateTime start,
            DateTime end,
            int phaseNumber,
            bool phaseOrOverlap,
            string phaseNumberSort,
            bool getPermissivePhase,
            List<ControllerEventLog> pedestrianIntervals,
            Dictionary<string, List<ControllerEventLog>> pedestrianEvents,
            Dictionary<string, List<ControllerEventLog>> cycleAllEvents,
            Dictionary<string, List<ControllerEventLog>> advanceCountEvents,
            Dictionary<string, List<ControllerEventLog>> advancePresenceEvents,
            Dictionary<string, List<ControllerEventLog>> stopBarEvents,
            Dictionary<string, List<ControllerEventLog>> laneByLanes,
            Dictionary<string, List<ControllerEventLog>> phaseCustomEvents) : base(approachId, signalId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseOrOverlap = phaseOrOverlap;
            PhaseNumberSort = phaseNumberSort;
            GetPermissivePhase = getPermissivePhase;
            PedestrianIntervals = pedestrianIntervals;
            PedestrianEvents = pedestrianEvents;
            CycleAllEvents = cycleAllEvents;
            AdvanceCountEvents = advanceCountEvents;
            AdvancePresenceEvents = advancePresenceEvents;
            StopBarEvents = stopBarEvents;
            LaneByLanes = laneByLanes;
            PhaseCustomEvents = phaseCustomEvents;
        }

        public int PhaseNumber { get; set; }
        public bool PhaseOrOverlap { get; set; }
        public string PhaseNumberSort { get; set; }
        public bool GetPermissivePhase { get; set; }
        public List<ControllerEventLog> PedestrianIntervals { get; set; }
        public Dictionary<string, List<ControllerEventLog>> PedestrianEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> CycleAllEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> AdvanceCountEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> AdvancePresenceEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> StopBarEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> LaneByLanes { get; set; }
        public Dictionary<string, List<ControllerEventLog>> PhaseCustomEvents { get; set; }
    }
}

