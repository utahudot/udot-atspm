#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.TimeSpaceDiagram/TimeSpaceDiagramResultForPhase.cs
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
