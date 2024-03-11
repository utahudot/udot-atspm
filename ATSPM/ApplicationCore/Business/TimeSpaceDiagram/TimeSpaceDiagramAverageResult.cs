using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.TimingAndActuation;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramAverageResult : ApproachResult
    {
        public TimeSpaceDiagramAverageResult(
            int approachId,
            string locationId,
            DateTime start,
            DateTime end,
            int phaseNumber,
            string phaseNumberSort,
            double distanceToNextLocation,
            int speed,
            int refPoint,
            int programmedSplit,
            bool coordinatedPhases,
            int cycleLength,
            List<CycleEventsDto> cycleAllEvents,
            List<TimeSpaceEventBase> greenTimeEvents) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseNumberSort = phaseNumberSort;
            DistanceToNextLocation = distanceToNextLocation;
            Speed = speed;
            Offset = refPoint;
            ProgrammedSplit = programmedSplit;
            CoordinatedPhases = coordinatedPhases;
            CycleLength = cycleLength;
            CycleAllEvents = cycleAllEvents;
            GreenTimeEvents = greenTimeEvents;
        }

        public bool CoordinatedPhases { get; set; }
        public int PhaseNumber { get; set; }
        public int Speed { get; set; }
        public int Offset { get; }
        public int ProgrammedSplit { get; set; }
        public string PhaseType { get; set; }
        public int CycleLength { get; }
        public string PhaseNumberSort { get; set; }
        public double DistanceToNextLocation { get; set; }
        public List<CycleEventsDto> CycleAllEvents { get; set; }
        public List<TimeSpaceEventBase> GreenTimeEvents { get; set; }
    }
}
