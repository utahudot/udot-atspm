using ATSPM.Data.Models;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.TimingAndActuation
{
    public class TimingAndActuationsForPhaseResult : ApproachResult
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
            List<DataPointEventCode> pedestrianIntervals,
            Dictionary<string, List<DataPointEventCode>> pedestrianEvents,
            Dictionary<string, List<ControllerEventLog>> cycleAllEvents,
            Dictionary<string, List<DataPointEventCode>> advanceCountEvents,
            Dictionary<string, List<DataPointEventCode>> advancePresenceEvents,
            Dictionary<string, List<DataPointEventCode>> stopBarEvents,
            Dictionary<string, List<DataPointEventCode>> laneByLanes,
            Dictionary<string, List<DataPointEventCode>> phaseCustomEvents) : base(approachId, signalId, start, end)
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
        public List<DataPointEventCode> PedestrianIntervals { get; set; }
        public Dictionary<string, List<DataPointEventCode>> PedestrianEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> CycleAllEvents { get; set; }
        public Dictionary<string, List<DataPointEventCode>> AdvanceCountEvents { get; set; }
        public Dictionary<string, List<DataPointEventCode>> AdvancePresenceEvents { get; set; }
        public Dictionary<string, List<DataPointEventCode>> StopBarEvents { get; set; }
        public Dictionary<string, List<DataPointEventCode>> LaneByLanes { get; set; }
        public Dictionary<string, List<DataPointEventCode>> PhaseCustomEvents { get; set; }
    }
}

