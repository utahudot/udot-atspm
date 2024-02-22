using ATSPM.Data.Models;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.PreemptService;
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
            IReadOnlyList<ControllerEventLog> planEvents,
            IReadOnlyList<ControllerEventLog> events)
        {
            var preemptEvents = events.Where(row => row.EventCode == 102).Select(row => new DataPointForInt(row.Timestamp, row.EventParam));
            var plans = planService.GetBasicPlans(options.Start, options.End, options.locationIdentifier, planEvents);
            IReadOnlyList<Plan> preemptPlans = plans.Select(pl => new PreemptPlan(
                pl.PlanNumber.ToString(),
                pl.Start,
                pl.End,
                preemptEvents.Count(p => p.Timestamp >= pl.Start && p.Timestamp < pl.End))).ToList();
            return new PreemptServiceRequestResult(
                "Preempt Service",
                options.locationIdentifier,
                options.Start,
                options.End,
                preemptPlans,
                preemptEvents.ToList()
                );
        }
    }
}