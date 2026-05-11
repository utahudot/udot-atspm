#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.ArrivalOnRed/ArrivalOnRedResult.cs
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

namespace Utah.Udot.Atspm.Business.PriorityDetails
{
    public class PriorityDetailsResult : ApproachResult
    {
        public PriorityDetailsResult(
            int approachId,
            string locationId,
            DateTime start,
            DateTime end,
            int phaseNumber,
            int? transitSignalPriorityNumber,
            bool phaseOrOverlap,
            string phaseNumberSort,
            string phaseType,

            double numberCheckins,
            double numberCheckouts,
            double numberEarlyGreens,
            double numberExtendedGreens,
            ICollection<IndianaEvent> indianaEvents,
            List<CycleEventsDto> cycleAllEvents,
            List<DetectorEventDto> priorityAndPreemptionEvents) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            TransitSignalPriorityNumber = transitSignalPriorityNumber;
            IsPhaseOverLap = phaseOrOverlap;
            PhaseNumberSort = phaseNumberSort;
            PhaseType = phaseType;
            NumberCheckins = numberCheckins;
            NumberCheckouts = numberCheckouts;
            NumberEarlyGreens = numberEarlyGreens;
            NumberExtendedGreens = numberExtendedGreens;
            CycleEvents = cycleAllEvents;
            PriorityAndPreemptionEvents = priorityAndPreemptionEvents;
            TspEvents = indianaEvents;
        }

        public int PhaseNumber { get; set; }
        public int? TransitSignalPriorityNumber { get; set; }
        public bool IsPhaseOverLap { get; set; }
        public string PhaseNumberSort { get; set; }
        public string PhaseType { get; set; }
        public double NumberCheckins { get; set; }
        public double NumberCheckouts { get; set; }
        public double NumberEarlyGreens { get; set; }
        public double NumberExtendedGreens { get; set; }
        public ICollection<IndianaEvent> TspEvents { get; set; }
        public List<CycleEventsDto> CycleEvents { get; set; }
        public List<DetectorEventDto> PriorityAndPreemptionEvents { get; set; }
    }
}