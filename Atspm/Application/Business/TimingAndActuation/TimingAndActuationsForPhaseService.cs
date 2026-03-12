#region license
// Copyright 2026 Utah Departement of Transportation
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

namespace Utah.Udot.Atspm.Business.TimingAndActuation
{
    public class TimingAndActuationsForPhaseService
    {
        private readonly CycleService _cycleService;
        private readonly DetectionService _detectionService;

        public TimingAndActuationsForPhaseService(
            DetectionService detectionService,
            CycleService cycleService)
        {
            this._detectionService = detectionService;
            this._cycleService = cycleService;
        }

        public TimingAndActuationsForPhaseResult GetChartData(
            TimingAndActuationsOptions options,
            PhaseDetail phaseDetail,
            List<IndianaEvent> controllerEventLogs,
            bool usePermissivePhase
            )
        {
            var pedestrianEvents = new List<DetectorEventDto>();
            var phaseCustomEvents = new Dictionary<string, List<DataPointForInt>>();
            var pedestrianIntervals = new List<CycleEventsDto>();

            if (!usePermissivePhase)
            {
                pedestrianEvents = _cycleService.GetPedestrianEventsNew(phaseDetail.Approach, options.Start, options.End, controllerEventLogs);
                pedestrianIntervals = _cycleService.GetPedestrianIntervals(phaseDetail.Approach, controllerEventLogs, options.Start, options.End);
            }

            var stopBarEvents = _detectionService.GetDetectionEvents(phaseDetail.Approach, options.Start, options.End, controllerEventLogs, DetectionTypes.SBP);
            var laneByLanes = _detectionService.GetDetectionEvents(phaseDetail.Approach, options.Start, options.End, controllerEventLogs, DetectionTypes.LLC);
            var advancePresenceEvents = _detectionService.GetDetectionEvents(phaseDetail.Approach, options.Start, options.End, controllerEventLogs, DetectionTypes.AP);
            var advanceCountEvents = _detectionService.GetDetectionEvents(phaseDetail.Approach, options.Start, options.End, controllerEventLogs, DetectionTypes.AC);

            if (options.PhaseEventCodesList != null)
            {
                phaseCustomEvents = _cycleService.GetPhaseCustomEvents(phaseDetail.Approach.Location.LocationIdentifier, phaseDetail.PhaseNumber, options.Start, options.End, options.PhaseEventCodesList, controllerEventLogs);
            }
            var cycleAllEvents = _cycleService.GetCycleEvents(phaseDetail, controllerEventLogs, options.Start, options.End);
            var phaseNumberSort = _cycleService.GetPhaseSort(phaseDetail);
            var timingAndActuationsForPhaseData = new TimingAndActuationsForPhaseResult(
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Location.LocationIdentifier,
                options.Start,
                options.End,
                phaseDetail.PhaseNumber,
                phaseDetail.UseOverlap,
                phaseNumberSort,
                usePermissivePhase ? "Permissive" : "Protected",
                pedestrianIntervals,
                pedestrianEvents,
                cycleAllEvents,
                advanceCountEvents,
                advancePresenceEvents,
                stopBarEvents,
                laneByLanes,
                phaseCustomEvents
                );
            return timingAndActuationsForPhaseData;
        }
    }
}

