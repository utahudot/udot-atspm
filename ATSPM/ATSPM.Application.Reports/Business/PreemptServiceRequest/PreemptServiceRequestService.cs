﻿using ATSPM.Application.Extensions;
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
        private readonly ISignalRepository signalRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public PreemptServiceRequestService(PlanService planService, ISignalRepository signalRepository, IControllerEventLogRepository controllerEventLogRepository)
        {
            this.planService = planService;
            this.signalRepository = signalRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public PreemptServiceRequestResult GetChartData(PreemptServiceRequestOptions options, List<ControllerEventLog> planEvents)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalId, options.Start);
            var events = controllerEventLogRepository.GetSignalEventsBetweenDates(options.SignalId, options.Start, options.End);
            var preemptEvents = GetPreemptEvents(events);
            var plans = planService.GetBasicPlans(options.Start, options.End, options.SignalId, planEvents);
            List<Common.Plan> preemptPlans = new List<Common.Plan>();
            foreach (var pl in plans)
            {
                preemptPlans.Add(new PreemptPlan(pl.PlanNumber.ToString(), pl.StartTime, pl.EndTime, preemptEvents.Count(p => p.StartTime >= pl.StartTime && p.StartTime < pl.EndTime)));
            }
            return new PreemptServiceRequestResult(
                "Preempt Service",
                options.SignalId,
                signal.SignalDescription(),
                options.Start,
                options.End,
                preemptPlans,
                preemptEvents
                );
        }


        protected List<PreemptRequest> GetPreemptEvents(IReadOnlyList<ControllerEventLog> events)
        {
            List<PreemptRequest> preemtpEvents = new List<PreemptRequest>();
            foreach (var row in events)
            {
                if (row.EventCode == 102)
                {
                    preemtpEvents.Add(new PreemptRequest(row.Timestamp, row.EventParam));
                }
            }
            return preemtpEvents;
        }
    }
}