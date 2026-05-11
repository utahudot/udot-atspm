#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimingAndActuation/TimingAndActuationsForPhaseService.cs
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
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.PriorityDetails
{
    public class PriorityDetailsService
    {
        private readonly CycleService _cycleService;
        private readonly DetectionService _detectionService;

        public PriorityDetailsService(
            DetectionService detectionService,
            CycleService cycleService)
        {
            this._detectionService = detectionService;
            this._cycleService = cycleService;
        }

        public PriorityDetailsResult GetChartData(
            PriorityDetailsOptions options,
            PhaseDetail phaseDetail,
            List<IndianaEvent> cycleEventLogs,
            List<IndianaEvent> priorityEventLogs,
            List<IndianaEvent> detectionEvents,
            bool usePermissivePhase)
        {
            var priorityAndPreemptionEvents = _detectionService.GetDetectionEvents(phaseDetail.Approach, options.Start, options.End, detectionEvents, DetectionTypes.PP);

            var cycleAllEvents = _cycleService.GetCycleEvents(phaseDetail, cycleEventLogs, options.Start, options.End);
            var phaseNumberSort = _cycleService.GetPhaseSort(phaseDetail);

            var numberCheckins = priorityEventLogs.Where(row => row.EventCode == 112).Count();
            var numberCheckouts = priorityEventLogs.Where(row => row.EventCode == 115).Count();
            var numberEarlyGreens = priorityEventLogs.Where(row => row.EventCode == 113).Count();
            var numberExtendedGreens = priorityEventLogs.Where(row => row.EventCode == 114).Count();

            var timingAndActuationsForPhaseData = new PriorityDetailsResult(
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Location.LocationIdentifier,
                options.Start,
                options.End,
                phaseDetail.PhaseNumber,
                phaseDetail.Approach.TransitSignalPriorityNumber,
                phaseDetail.UseOverlap,
                phaseNumberSort,
                usePermissivePhase ? "Permissive" : "Protected",
                numberCheckins,
                numberCheckouts,
                numberEarlyGreens,
                numberExtendedGreens,
                priorityEventLogs,
                cycleAllEvents,
                priorityAndPreemptionEvents
                );
            return timingAndActuationsForPhaseData;
        }

    }
}

