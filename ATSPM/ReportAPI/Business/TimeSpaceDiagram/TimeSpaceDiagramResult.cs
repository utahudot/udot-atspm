using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.TimingAndActuation;

namespace ATSPM.ReportApi.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramResult : ApproachResult
    {
        public TimeSpaceDiagramResult(
            int approachId,
            string locationId,
            DateTime start,
            DateTime end,
            int phaseNumber,
            string phaseNumberSort,
            double distanceToNextLocation,
            List<CycleEventsDto> cycleAllEvents,
            List<TimeSpaceEventBase> laneByLaneCountDetectors,
            List<TimeSpaceEventBase> advanceCountDetectors,
            List<TimeSpaceEventBase> stopBarPresenceDetectors,
            List<TimeSpaceEventBase> greenTimeEvents) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseNumberSort = phaseNumberSort;
            DistanceToNextLocation = distanceToNextLocation;
            CycleAllEvents = cycleAllEvents;
            LaneByLaneCountDetectors = laneByLaneCountDetectors;
            AdvanceCountDetectors = advanceCountDetectors;
            StopBarPresenceDetectors = stopBarPresenceDetectors;
            GreenTimeEvents = greenTimeEvents;
        }

        public int PhaseNumber { get; set; }
        public string PhaseNumberSort { get; set; }
        public double DistanceToNextLocation { get; set; }
        public List<CycleEventsDto> CycleAllEvents { get; set; }
        public List<TimeSpaceEventBase> GreenTimeEvents { get; set; }
        public List<TimeSpaceEventBase> LaneByLaneCountDetectors { get; set; }
        public List<TimeSpaceEventBase> AdvanceCountDetectors { get; set; }
        public List<TimeSpaceEventBase> StopBarPresenceDetectors { get; set; }
    }
}
