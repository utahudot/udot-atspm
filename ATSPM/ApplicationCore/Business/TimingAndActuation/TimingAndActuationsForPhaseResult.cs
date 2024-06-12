#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.TimingAndActuation/TimingAndActuationsForPhaseResult.cs
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
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.TimingAndActuation
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
            string phaseType,
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
            IsPhaseOverLap = phaseOrOverlap;
            PhaseNumberSort = phaseNumberSort;
            PhaseType = phaseType;
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
        public bool IsPhaseOverLap { get; set; }
        public string PhaseNumberSort { get; set; }
        public string PhaseType { get; set; }
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