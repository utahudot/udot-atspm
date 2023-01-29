using ATSPM.Application.Repositories;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Application.Reports.ViewModels.PreemptService;
using Legacy.Common.Business.WCFServiceLibrary;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using System;

namespace Legacy.Common.Business.Preempt
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

        public PreemptServiceResult GetChartData(string signalId, DateTime startDate, DateTime endDate)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(signalId, startDate);
            var events= controllerEventLogRepository.GetSignalEventsBetweenDates(signalId, startDate, endDate);
            var preemptEvents = GetPreemptEvents(events);
            var plans = planService.GetBasicPlans(startDate, endDate, signalId);
            List<PreemptPlan> preemptPlans = new List<PreemptPlan>();   
            foreach(var pl in plans)
            {
                preemptPlans.Add(new PreemptPlan(pl.PlanNumber.ToString(), pl.StartTime, pl.EndTime, preemptEvents.Count(p => p.StartTime >= pl.StartTime && p.StartTime < pl.EndTime)));
            }
            return new PreemptServiceResult(
                "Preempt Service",
                signalId,
                signal.SignalDescription(),
                startDate,
                endDate,
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