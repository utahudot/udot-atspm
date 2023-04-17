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
        private readonly ISignalRepository signalRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public PreemptServiceService(PlanService planService, ISignalRepository signalRepository, IControllerEventLogRepository controllerEventLogRepository)
        {
            this.planService = planService;
            this.signalRepository = signalRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public PreemptServiceResult GetChartData(
            PreemptServiceMetricOptions options,
            List<ControllerEventLog> planEvents)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalId, options.Start);
            var events = controllerEventLogRepository.GetSignalEventsBetweenDates(options.SignalId, options.Start, options.End);
            var preemptEvents = GetPreemptEvents(events);
            var plans = planService.GetBasicPlans(options.Start, options.End, options.SignalId, planEvents);
            List<PreemptPlan> preemptPlans = new List<PreemptPlan>();
            foreach (var pl in plans)
            {
                preemptPlans.Add(new PreemptPlan(pl.PlanNumber.ToString(), pl.StartTime, pl.EndTime, preemptEvents.Count(p => p.StartTime >= pl.StartTime && p.StartTime < pl.EndTime)));
            }
            return new PreemptServiceResult(
                "Preempt Service",
                options.SignalId,
                signal.SignalDescription(),
                options.Start,
                options.End,
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