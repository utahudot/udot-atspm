using Reports.Business.Common;
using Reports.Business.TimingAndActuation;
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
            PedestrianDetectors = pedestrianEvents;
            CycleEvents = cycleAllEvents;
            AdvanceCountDetectors = advanceCountEvents;
            AdvancePresenceDetectors = advancePresenceEvents;
            StopBarDetectors = stopBarEvents;
            LaneByLanesDetectors = laneByLanes;
            PhaseCustomEvents = phaseCustomEvents;
        }

        public int PhaseNumber { get; set; }
        public bool PhaseOrOverlap { get; set; }
        public string PhaseNumberSort { get; set; }
        public bool GetPermissivePhase { get; set; }
        public List<DataPointForDetectorEvent> PedestrianIntervals { get; set; }
        public Dictionary<string, List<DetectorEventDto>> PedestrianDetectors { get; set; }
        public Dictionary<string, List<CycleEventsDto>> CycleEvents { get; set; }
        public Dictionary<string, List<DetectorEventDto>> AdvanceCountDetectors { get; set; }
        public Dictionary<string, List<DetectorEventDto>> AdvancePresenceDetectors { get; set; }
        public List<DetectorEventDto> StopBarDetectors { get; set; }
        public List<DetectorEventDto> LaneByLanesDetectors { get; set; }
        public Dictionary<string, List<DataPointForInt>> PhaseCustomEvents { get; set; }
    }
}

