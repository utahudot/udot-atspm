using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.TimingAndActuation
{
    public class TimingAndActuationsForPhaseResult : ApproachResult
    {
        public TimingAndActuationsForPhaseResult(
            int approachId,
            string locationId,
            DateTime start,
            DateTime end,
            int phaseNumber,
            bool phaseOrOverlap,
            string phaseNumberSort,
            bool getPermissivePhase,
            List<CycleEventsDto> pedestrianIntervals,
            List<DetectorEventDto> pedestrianEvents,
            List<CycleEventsDto> cycleAllEvents,
            List<DetectorEventDto> advanceCountEvents,
            List<DetectorEventDto> advancePresenceEvents,
            List<DetectorEventDto> stopBarEvents,
            List<DetectorEventDto> laneByLanes,
            Dictionary<string, List<DataPointForInt>> phaseCustomEvents) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseOrOverlap = phaseOrOverlap;
            PhaseNumberSort = phaseNumberSort;
            GetPermissivePhase = getPermissivePhase;
            PedestrianIntervals = pedestrianIntervals;
            PedestrianEvents = pedestrianEvents;
            CycleAllEvents = cycleAllEvents;
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
        public List<CycleEventsDto> PedestrianIntervals { get; set; }
        public List<DetectorEventDto> PedestrianEvents { get; set; }
        public List<CycleEventsDto> CycleAllEvents { get; set; }
        public List<DetectorEventDto> AdvanceCountDetectors { get; set; }
        public List<DetectorEventDto> AdvancePresenceDetectors { get; set; }
        public List<DetectorEventDto> StopBarDetectors { get; set; }
        public List<DetectorEventDto> LaneByLanesDetectors { get; set; }
        public Dictionary<string, List<DataPointForInt>> PhaseCustomEvents { get; set; }
    }
}