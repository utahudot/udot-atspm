#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeSpaceDiagram/TimeSpaceDiagramResultForPhase.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.TimeSpaceDiagram
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
            double distanceToPreviousLocation,
            int speed,
            int programmedCycleLength,
            List<CycleEventsDto> cycleAllEvents,
            List<CycleEventsDto> pedIntervals,
            List<TimeSpaceDetectorEventDto> laneByLaneCountDetectors,
            List<TimeSpaceDetectorEventDto> advanceCountDetectors,
            List<TimeSpaceDetectorEventDto> stopBarPresenceDetectors,
            List<DataPointWithDetectorCheckBase> greenTimeEvents,
            bool phaseOrOverlap,
            double tspnNumberCheckins,
            double tspNumberCheckouts,
            double tspNumberEarlyGreens,
            double tspNumberExtendedGreens,
            ICollection<IndianaEvent> tspEvents,
            List<DetectorEventDto> priorityAndPreemptionEvents) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseNumberSort = phaseNumberSort;
            DistanceToNextLocation = distanceToNextLocation;
            DistanceToPreviousLocation = distanceToPreviousLocation;
            Speed = speed;
            CycleLength = programmedCycleLength;
            CycleAllEvents = cycleAllEvents;
            PedestrianIntervals = pedIntervals;
            LaneByLaneCountDetectors = laneByLaneCountDetectors;
            AdvanceCountDetectors = advanceCountDetectors;
            StopBarPresenceDetectors = stopBarPresenceDetectors;
            GreenTimeEvents = greenTimeEvents;
            IsPhaseOverLap = phaseOrOverlap;
            TSPNumberCheckins = tspnNumberCheckins;
            TSPNumberCheckouts = tspNumberCheckouts;
            TSPNumberEarlyGreens = tspNumberEarlyGreens;
            TSPNumberExtendedGreens = tspNumberExtendedGreens;
            PriorityAndPreemptionEvents = priorityAndPreemptionEvents;
            TspEvents = tspEvents;
        }

        public int PhaseNumber { get; set; }
        public int Speed { get; set; }
        public string PhaseType { get; set; }
        public string PhaseNumberSort { get; set; }
        public double DistanceToNextLocation { get; set; }
        public double DistanceToPreviousLocation { get; set; }
        public double PercentArrivalOnGreen { get; set; }
        public int Order { get; set; }
        public int CycleLength { get; }
        public TmcForPhaseDto TmcForPhase { get; set; }
        public List<CycleEventsDto> CycleAllEvents { get; set; }
        public List<CycleEventsDto> PedestrianIntervals { get; set; }
        public List<DataPointWithDetectorCheckBase> GreenTimeEvents { get; set; }
        public List<TimeSpaceDetectorEventDto> LaneByLaneCountDetectors { get; set; }
        public List<TimeSpaceDetectorEventDto> AdvanceCountDetectors { get; set; }
        public List<TimeSpaceDetectorEventDto> StopBarPresenceDetectors { get; set; }
        public bool IsPhaseOverLap { get; set; }
        public double TSPNumberCheckins { get; set; }
        public double TSPNumberCheckouts { get; set; }
        public double TSPNumberEarlyGreens { get; set; }
        public double TSPNumberExtendedGreens { get; set; }
        public ICollection<IndianaEvent> TspEvents { get; set; }
        public List<DetectorEventDto> PriorityAndPreemptionEvents { get; set; }
    }
}
