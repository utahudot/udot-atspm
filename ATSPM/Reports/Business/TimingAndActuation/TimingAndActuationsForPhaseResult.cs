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
            List<DataPointForInt> pedestrianIntervals,
            Dictionary<string, List<DataPointForInt>> pedestrianEvents,
            Dictionary<string, List<DataPointForInt>> cycleAllEvents,
            Dictionary<string, List<DataPointForInt>> advanceCountEvents,
            Dictionary<string, List<DataPointForInt>> advancePresenceEvents,
            Dictionary<string, List<DataPointForInt>> stopBarEvents,
            Dictionary<string, List<DataPointForInt>> laneByLanes,
            Dictionary<string, List<DataPointForInt>> phaseCustomEvents) : base(approachId, signalId, start, end)
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
        public List<DataPointForInt> PedestrianIntervals { get; set; }
        public Dictionary<string, List<DataPointForInt>> PedestrianEvents { get; set; }
        public Dictionary<string, List<DataPointForInt>> CycleAllEvents { get; set; }
        public Dictionary<string, List<DataPointForInt>> AdvanceCountEvents { get; set; }
        public Dictionary<string, List<DataPointForInt>> AdvancePresenceEvents { get; set; }
        public Dictionary<string, List<DataPointForInt>> StopBarEvents { get; set; }
        public Dictionary<string, List<DataPointForInt>> LaneByLanes { get; set; }
        public Dictionary<string, List<DataPointForInt>> PhaseCustomEvents { get; set; }
    }
}

