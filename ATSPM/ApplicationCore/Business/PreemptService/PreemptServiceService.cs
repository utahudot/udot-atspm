﻿using ATSPM.Data.Models;
using ATSPM.Application.Business.Common;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.PreemptService
{
    public class PreemptServiceService
    {
        private readonly PlanService planService;
        public PreemptServiceService(PlanService planService)
        {
            this.planService = planService;
        }

        public PreemptServiceResult GetChartData(
            PreemptServiceOptions options,
            IReadOnlyList<ControllerEventLog> planEvents,
            IReadOnlyList<ControllerEventLog> preemptEvents)
        {
            IReadOnlyList<Plan> plans = planService.GetBasicPlans(options.Start, options.End, options.locationIdentifier, planEvents);
            var preemptPlans = plans.Select(pl => new PreemptPlan(
                pl.PlanNumber.ToString(),
                pl.Start,
                pl.End,
                preemptEvents.Count(p => p.EventCode == 105 && p.Timestamp >= pl.Start && p.Timestamp < pl.End))).ToList();

            return new PreemptServiceResult(
                options.locationIdentifier,
                options.Start,
                options.End,
                preemptPlans,
                preemptEvents.Select(p => new DataPointForInt(p.Timestamp, p.EventParam)).ToList()
                );
        }
    }
}