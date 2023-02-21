using ATSPM.Application.Repositories;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using System;
using Legacy.Common.Business;

namespace ATSPM.Application.Reports.Business.PreemptService
{
    public class PreemptServiceService
    {
        private readonly PlanService planService;
        private readonly ISignalRepository signalRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public PreemptServiceService(PlanService planService, ISignalRepository signalRepository, IControllerEventLogRepository controllerEventLogRepository)
        {
            this.planService = planService;
            this.signalRepository = signalRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public PreemptServiceResult GetChartData(PreemptServiceMetricOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalId, options.StartDate);
            var events = controllerEventLogRepository.GetSignalEventsBetweenDates(options.SignalId, options.StartDate, options.EndDate);
            var preemptEvents = GetPreemptEvents(events);
            var plans = planService.GetBasicPlans(options.StartDate, options.EndDate, options.SignalId);
            List<PreemptPlan> preemptPlans = new List<PreemptPlan>();
            foreach (var pl in plans)
            {
                preemptPlans.Add(new PreemptPlan(pl.PlanNumber.ToString(), pl.StartTime, pl.EndTime, preemptEvents.Count(p => p.StartTime >= pl.StartTime && p.StartTime < pl.EndTime)));
            }
            return new PreemptServiceResult(
                "Preempt Service",
                options.SignalId,
                signal.SignalDescription(),
                options.StartDate,
                options.EndDate,
                preemptPlans,
                preemptEvents
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