#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/LocationPhaseService.cs
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
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Application.Business.Common
{
    public class LocationPhaseService
    {
        private readonly PlanService planService;
        private readonly CycleService cycleService;
        private readonly ILogger<LocationPhaseService> logger;

        public LocationPhaseService(
            PlanService planService,
            CycleService cycleService,
            ILogger<LocationPhaseService> logger
            )
        {
            this.planService = planService;
            this.cycleService = cycleService;
            this.logger = logger;
        }



        public void LinkPivotAddSeconds(LocationPhase LocationPhase, int seconds)
        {
            LocationPhase.ResetVolume();
            foreach (var cyclePcd in LocationPhase.Cycles)
            {
                cyclePcd.AddSecondsToDetectorEvents(seconds);
            }
        }

        /// <summary>
        /// Needs event codes 1,8,9,61,63,64,131,82
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="getPermissivePhase"></param>
        /// <param name="showVolume"></param>
        /// <param name="pcdCycleTime"></param>
        /// <param name="binSize"></param>
        /// <param name="approach"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        public async Task<LocationPhase> GetLocationPhaseData(
            PhaseDetail phaseDetail,
            DateTime start,
            DateTime end,
            bool showVolume,
            int? pcdCycleTime,
            int binSize,
            List<IndianaEvent> cycleEvents,
            List<IndianaEvent> planEvents,
            List<IndianaEvent> detectorEvents)
        {
            if (phaseDetail == null || phaseDetail.Approach == null)
            {
                logger.LogError("Approach cannot be null");
                throw new ArgumentNullException("Approach cannot be null");
            }

            if (!cycleEvents.Any())
                return new LocationPhase();
            var cycles = await cycleService.GetPcdCycles(start, end, detectorEvents, cycleEvents, pcdCycleTime);
            var plans = planService.GetPcdPlans(cycles, start, end, phaseDetail.Approach, planEvents);
            return new LocationPhase(
                showVolume ? new VolumeCollection(start, end, detectorEvents, binSize) : null,
                plans,
                cycles,
                detectorEvents,
                phaseDetail.Approach,
                start,
                end
                );
        }

        public async Task<LocationPhase> GetLocationPhaseDataWithApporach(
            Approach approach,
            DateTime start,
            DateTime end,
            bool showVolume,
            int? pcdCycleTime,
            int binSize,
            List<IndianaEvent> cycleEvents,
            List<IndianaEvent> planEvents,
            List<IndianaEvent> detectorEvents)
        {
            if (approach == null)
            {
                logger.LogError("Approach cannot be null");
                throw new ArgumentNullException("Approach cannot be null");
            }

            if (!cycleEvents.Any())
                return new LocationPhase();
            var cycles = await cycleService.GetPcdCycles(start, end, detectorEvents, cycleEvents, pcdCycleTime);
            var plans = planService.GetPcdPlans(cycles, start, end, approach, planEvents);
            return new LocationPhase(
                showVolume ? new VolumeCollection(start, end, detectorEvents, binSize) : null,
                plans,
                cycles,
                detectorEvents,
                approach,
                start,
                end
                );
        }

        public async Task<LocationPhase> GetLocationPhaseDataWithApproach(
           Approach approach,
           DateTime start,
           DateTime end,
           int binSize,
           int metricTypeId,
           List<IndianaEvent> controllerEventLogs,
           List<IndianaEvent> planEvents,
           bool getVolume,
           DetectionType detectionType,
           int? cycleTime)
        {
            var detectorEvents = controllerEventLogs.GetDetectorEvents(
                metricTypeId,
                approach,
                start,
                end,
                true,
                false,
                detectionType);
            if (detectorEvents == null)
            {
                return null;
            }

            var cycleEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(
                approach.ProtectedPhaseNumber,
                approach.IsProtectedPhaseOverlap,
                start,
                end);
            if (cycleEvents.IsNullOrEmpty())
                return null;
            var LocationPhase = await GetLocationPhaseDataWithApporach(
                approach,
                start,
                end,
                getVolume,
                cycleTime ?? null,
                binSize,
                cycleEvents.ToList(),
                planEvents.ToList(),
                detectorEvents.ToList());
            return LocationPhase;
        }

        public async Task<LocationPhase> GetLocationPhaseData(
            PhaseDetail phaseDetail,
            DateTime start,
            DateTime end,
            int binSize,
            DetectionType detectionType,
            List<IndianaEvent> controllerEventLogs,
            List<IndianaEvent> planEvents,
            bool getVolume)
        {
            var detectorEvents = controllerEventLogs.GetDetectorEvents(
                8,
                phaseDetail.Approach,
                start,
                end,
                true,
                false,
                detectionType);
            if (detectorEvents == null)
            {
                return null;
            }

            var cycleEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(
                phaseDetail.PhaseNumber,
                phaseDetail.UseOverlap,
                start,
                end);
            if (cycleEvents.IsNullOrEmpty())
                return null;
            var LocationPhase = await GetLocationPhaseData(
                phaseDetail,
                start,
                end,
                getVolume,
                null,
                binSize,
                cycleEvents.ToList(),
                planEvents.ToList(),
                detectorEvents.ToList());
            return LocationPhase;
        }
    }
}