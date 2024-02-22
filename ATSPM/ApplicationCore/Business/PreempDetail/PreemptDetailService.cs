using ATSPM.Data.Models;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.PreemptService;
using ATSPM.Application.TempModels;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.PreempDetail
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
            PreemptRequestAndServices preemptSummary = new PreemptRequestAndServices(preemptDetailOptions.locationIdentifier,
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
                var cycleToCycleResult = createResultCycles(cycles, preemptDetailOptions);

                var requests = requestEventsForNumber.Select(e => e.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss")).ToList();
                var services = serviceEventsForNumber.Select(e => e.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss")).ToList();
                requestAndServices.Add(new RequestAndServices() { PreemptionNumber = preemptNumber, Requests = requests, Services = services });

                preemptDetails.Add(new PreemptDetail(
                    preemptDetailOptions.locationIdentifier,
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

        private List<PreemptCycleResult> createResultCycles(List<PreemptCycle> cycles, PreemptDetailOptions options)
        {
            var result = cycles.Select(c => new PreemptCycleResult()
            {
                InputOff = c.InputOff[0],
                InputOn = c.EntryStarted >= options.Start && c.EntryStarted <= options.End ? c.EntryStarted : c.InputOn[0],
                GateDown = c.GateDown >= options.Start && c.GateDown <= options.End ? c.GateDown : null,
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