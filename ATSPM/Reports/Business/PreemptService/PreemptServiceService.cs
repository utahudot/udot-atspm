using ATSPM.Application.Repositories;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using System;
using ATSPM.Application.Reports.Business.Common;

namespace ATSPM.Application.Reports.Business.PreemptService
{
    public class PreemptServiceService
    {
        private readonly PlanService planService;
        public PreemptServiceService(PlanService planService)
        {
            this.planService = planService;
        }

        public PreemptServiceResult GetChartData(
            PreemptServiceMetricOptions options,
            IReadOnlyList<ControllerEventLog> planEvents,
            IReadOnlyList<ControllerEventLog> preemptEvents)
        {
            IReadOnlyList<Plan> plans = planService.GetBasicPlans(options.Start, options.End, options.SignalIdentifier, planEvents);
            var preemptPlans = plans.Select(pl => new PreemptPlan(
                pl.PlanNumber.ToString(),
                pl.Start,
                pl.End,
                preemptEvents.Count(p => p.EventCode == 105 && p.Timestamp >= pl.Start && p.Timestamp < pl.End))).ToList();

            return new PreemptServiceResult(
                options.SignalIdentifier,
                options.Start,
                options.End,
                preemptPlans,
                preemptEvents.Select(p => new PreemptServiceEvent(p.Timestamp, p.EventParam)).ToList()  
                );
        }
    }
}