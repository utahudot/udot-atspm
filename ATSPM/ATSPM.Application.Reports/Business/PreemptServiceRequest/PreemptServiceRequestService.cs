using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PreemptServiceRequest;
using ATSPM.Application.Repositories;
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

            var plans = planService.GetBasicPlans(options.Start, options.End, options.SignalId, planEvents);
            List<Common.Plan> preemptPlans = new List<Common.Plan>();
            foreach (var pl in plans)
            {
                preemptPlans.Add(new PreemptPlan(pl.PlanNumber.ToString(), pl.StartTime, pl.EndTime, preemptEvents.Count(p => p.StartTime >= pl.StartTime && p.StartTime < pl.EndTime)));
            }
            return new PreemptServiceRequestResult(
                "Preempt Service",
                options.SignalId,
                options.Start,
                options.End,
                preemptPlans,
                preemptEvents.ToList()
                );
        }
    }
}