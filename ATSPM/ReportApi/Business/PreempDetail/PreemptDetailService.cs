﻿using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.PreemptService;
using ATSPM.ReportApi.TempModels;

namespace ATSPM.ReportApi.Business.PreempDetail
{
    public class PreemptDetailService
    {
        private readonly CycleService cycleService;

        public PreemptDetailService(
            CycleService cycleService)
        {
            this.cycleService = cycleService;
        }

        public PreemptDetailResult GetChartData(
            PreemptDetailOptions preemptDetailOptions,
            List<ControllerEventLog> preemptEvents)
        {
            var preemptServiceEventCodes = new List<int>() { 105 };
            var preemptServiceRequestEventCodes = new List<int>() { 102 };

            var uniquePreemptNumbers = preemptEvents.Select(x => x.EventParam).Distinct().ToList();
            var preemptDetails = new List<PreemptDetail>();
            PreemptRequestAndServices preemptSummary = new PreemptRequestAndServices(preemptDetailOptions.SignalIdentifier,
                preemptDetailOptions.Start,
                preemptDetailOptions.End);
            var requestAndServices = new List<RequestAndServices>();

            var preemptServiceEvents = preemptEvents.Where(e => preemptServiceEventCodes.Contains(e.EventCode));
            var preemptServiceRequestEvents = preemptEvents.Where(e => preemptServiceRequestEventCodes.Contains(e.EventCode));


            foreach (var preemptNumber in uniquePreemptNumbers)
            {
                var serviceEventsForNumber = preemptServiceEvents.Where(e => e.EventParam == preemptNumber).ToList();
                var requestEventsForNumber = preemptServiceRequestEvents.Where(e => e.EventParam == preemptNumber).ToList();
                var tempEvents = preemptEvents
                    .Where(x => x.EventParam == preemptNumber).ToList();
                var cycles = cycleService.CreatePreemptCycle(tempEvents);
                var cycleToCycleResult = createResultCycles(cycles);

                var requests = requestEventsForNumber.Select(e => e.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss")).ToList();
                var services = serviceEventsForNumber.Select(e => e.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss")).ToList();
                requestAndServices.Add(new RequestAndServices() { PreemptionNumber = preemptNumber, Requests = requests, Services = services });

                preemptDetails.Add(new PreemptDetail(
                    preemptDetailOptions.SignalIdentifier,
                    preemptDetailOptions.Start,
                    preemptDetailOptions.End,
                    preemptNumber,
                    cycleToCycleResult));
            }

            preemptSummary.RequestAndServices = requestAndServices;

            return new PreemptDetailResult(
                preemptDetails,
                preemptSummary
                );
        }

        private List<PreemptCycleResult> createResultCycles(List<PreemptCycle> cycles)
        {
            var result = cycles.Select(c => new PreemptCycleResult()
            {
                InputOff = c.InputOff[0],
                InputOn = c.StartInputOn,
                GateDown = c.GateDown,
                CallMaxOut = c.TimeToCallMaxOut,
                Delay = c.Delay,
                TimeToService = c.TimeToService,
                DwellTime = c.DwellTime,
                TrackClear = c.TimeToTrackClear,
            }).ToList();

            return result;
        }
    }
}