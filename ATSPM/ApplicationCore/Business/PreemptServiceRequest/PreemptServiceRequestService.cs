#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PreemptServiceRequest/PreemptServiceRequestService.cs
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
using ATSPM.Application.Business.PreemptService;
using ATSPM.Data.Enums;
using ATSPM.Data.Models.EventLogModels;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.PreemptServiceRequest
{
    public class PreemptServiceRequestService
    {
        private readonly PlanService planService;

        public PreemptServiceRequestService(PlanService planService)
        {
            this.planService = planService;
        }

        public PreemptServiceRequestResult GetChartData(
            PreemptServiceRequestOptions options,
            IReadOnlyList<IndianaEvent> planEvents,
            IReadOnlyList<IndianaEvent> events)
        {
            var preemptEvents = events.Where(row => row.EventCode == 102).Select(row => new DataPointForInt(row.Timestamp, row.EventParam));
            var plans = planService.GetBasicPlans(options.Start, options.End, options.LocationIdentifier, planEvents);
            IReadOnlyList<Common.Plan> preemptPlans = plans.Select(pl => new PreemptPlan(
                pl.PlanNumber.ToString(),
                pl.Start,
                pl.End,
                preemptEvents.Count(p => p.Timestamp >= pl.Start && p.Timestamp < pl.End))).ToList();
            return new PreemptServiceRequestResult(
                "Preempt Service",
                options.LocationIdentifier,
                options.Start,
                options.End,
                preemptPlans,
                preemptEvents.ToList()
                );
        }
    }
}