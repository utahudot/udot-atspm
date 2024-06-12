#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.YellowRedActivations/YellowRedActivationsService.cs
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
using ATSPM.Application.Business.PedDelay;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.YellowRedActivations
{
    public class YellowRedActivationsService
    {
        private readonly PlanService planService;
        private readonly CycleService cycleService;

        public YellowRedActivationsService(
            PlanService planService,
            CycleService cycleService)
        {
            this.planService = planService;
            this.cycleService = cycleService;
        }


        public YellowRedActivationsResult GetChartData(
            YellowRedActivationsOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<IndianaEvent> cycleEvents,
            IReadOnlyList<IndianaEvent> detectorEvents,
            IReadOnlyList<IndianaEvent> planEvents)
        {

            var cycles = cycleService.GetYellowRedActivationsCycles(
                options.Start,
                options.End,
                cycleEvents,
                detectorEvents,
                options.SevereLevelSeconds
                );

            var plans = planService.GetYellowRedActivationPlans(
                options.Start,
                options.End,
                cycles,
                phaseDetail.Approach.Location.LocationIdentifier,
                options.SevereLevelSeconds,
                planEvents).ToList();

            var detectorActivations = cycles.SelectMany(c => c.DetectorActivations).ToList();

            var phaseType = phaseDetail.Approach.GetPhaseType().ToString();

            return new YellowRedActivationsResult(
                phaseDetail.Approach.Location.LocationIdentifier,
                phaseDetail.Approach.Id,
                phaseDetail.Approach.DirectionType.Abbreviation,
                phaseDetail.Approach.Detectors.FirstOrDefault().MovementType.ToString(),
                phaseDetail.Approach.ProtectedPhaseNumber,
                phaseDetail.Approach.PermissivePhaseNumber,
                phaseDetail.IsPermissivePhase,
                phaseType,
                options.Start,
                options.End,
                Convert.ToInt32(plans.Sum(p => p.Violations)),
                Convert.ToInt32(plans.Sum(p => p.SevereRedLightViolations)),
                Convert.ToInt32(plans.Sum(p => p.YellowOccurrences)),
                plans.Select(p => new YellowRedActivationsPlan(
                    p.PlanNumber.ToString(),
                    p.Start,
                    p.End,
                    Convert.ToInt32(p.Violations),
                    Convert.ToInt32(p.SevereRedLightViolations),
                    p.PercentViolations,
                    p.PercentSevereViolations,
                    p.AverageTRLV)).ToList(),
                cycles.Select(c => new DataPointForDouble(c.RedEvent, c.EndTime)).ToList(),
                cycles.Select(c => new DataPointForDouble(c.StartTime, c.RedClearanceEvent)).ToList(),
                cycles.Select(c => new DataPointForDouble(c.RedClearanceEvent, c.RedEvent)).ToList(),
                detectorActivations.Select(d => new DataPointForDouble(d.TimeStamp, d.YPoint)).ToList()
                );
        }
    }
}