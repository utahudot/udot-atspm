using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.TimingAndActuation;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramResultForPhase : ApproachResult
    {
        public TimeSpaceDiagramResultForPhase(
            int approachId,
            string locationId,
            DateTime start,
            DateTime end,
            int phaseNumber,
            string phaseNumberSort,
            double distanceToNextLocation,
            int speed,
            List<CycleEventsDto> cycleAllEvents,
            List<TimeSpaceEventBase> laneByLaneCountDetectors,
            List<TimeSpaceEventBase> advanceCountDetectors,
            List<TimeSpaceEventBase> stopBarPresenceDetectors,
            List<TimeSpaceEventBase> greenTimeEvents) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseNumberSort = phaseNumberSort;
            DistanceToNextLocation = distanceToNextLocation;
            Speed = speed;
            CycleAllEvents = cycleAllEvents;
            LaneByLaneCountDetectors = laneByLaneCountDetectors;
            AdvanceCountDetectors = advanceCountDetectors;
            StopBarPresenceDetectors = stopBarPresenceDetectors;
            GreenTimeEvents = greenTimeEvents;
        }

        public int PhaseNumber { get; set; }
        public int Speed { get; set; }
        public string PhaseType { get; set; }
        public string PhaseNumberSort { get; set; }
        public double DistanceToNextLocation { get; set; }
        public List<CycleEventsDto> CycleAllEvents { get; set; }
        public List<TimeSpaceEventBase> GreenTimeEvents { get; set; }
        public List<TimeSpaceEventBase> LaneByLaneCountDetectors { get; set; }
        public List<TimeSpaceEventBase> AdvanceCountDetectors { get; set; }
        public List<TimeSpaceEventBase> StopBarPresenceDetectors { get; set; }
    }
}
