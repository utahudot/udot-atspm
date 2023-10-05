using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PreemptServiceRequest;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.PreemptService
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
            var preemptEvents = events.Where(row => row.EventCode == 102).Select(row => new PreemptRequest(row.Timestamp, row.EventParam));
            var plans = planService.GetBasicPlans(options.Start, options.End, options.SignalIdentifier, planEvents);
            IReadOnlyList<Plan> preemptPlans = plans.Select(pl => new PreemptPlan(
                pl.PlanNumber.ToString(),
                pl.Start,
                pl.End,
                preemptEvents.Count(p => p.StartTime >= pl.Start && p.StartTime < pl.End))).ToList();
            return new PreemptServiceRequestResult(
                "Preempt Service",
                options.SignalIdentifier,
                options.Start,
                options.End,
                preemptPlans,
                preemptEvents.ToList()
                );
        }
    }
}