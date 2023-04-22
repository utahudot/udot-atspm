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
            List<ControllerEventLog> planEvents,
            List<ControllerEventLog> preemptEvents)
        {
            var plans = planService.GetBasicPlans(options.Start, options.End, options.SignalId, planEvents);
            List<PreemptPlan> preemptPlans = new List<PreemptPlan>(); 
            preemptPlans = plans.Select(pl => new PreemptPlan(
                pl.PlanNumber.ToString(),
                pl.StartTime,
                pl.EndTime,
                preemptEvents.Count(p => p.EventCode == 105 && p.Timestamp >= pl.StartTime && p.Timestamp < pl.EndTime))).ToList();

            return new PreemptServiceResult(
                "Preempt Service",
                options.SignalId,
                options.Start,
                options.End,
                preemptPlans,
                preemptEvents.Select(p => new PreemptServiceEvent(p.Timestamp, p.EventParam)).ToList()  
                );
        }




        protected List<PreemptServiceEvent> GetPreemptEvents(IReadOnlyList<ControllerEventLog> events)
        {
            List<PreemptServiceEvent> preemtpEvents = new List<PreemptServiceEvent>();
            foreach (var row in events)
            {
                if (row.EventCode == 105)
                {
                    preemtpEvents.Add(new PreemptServiceEvent(row.Timestamp, row.EventParam));
                }
            }
            return preemtpEvents;
        }
    }
}