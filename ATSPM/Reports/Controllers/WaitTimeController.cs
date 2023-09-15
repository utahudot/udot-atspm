using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.WaitTime;
using ATSPM.Application.Repositories;
using AutoFixture;
//using Legacy.Common.Business;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaitTimeController : ControllerBase
    {
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly WaitTimeService waitTimeService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public WaitTimeController(
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            WaitTimeService waitTimeService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository
            )
        {
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.waitTimeService = waitTimeService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public WaitTimeResult Test()
        {
            Fixture fixture = new();
            WaitTimeResult viewModel = fixture.Create<WaitTimeResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public async Task<IEnumerable<WaitTimeResult>> GetChartData([FromBody] WaitTimeOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var phaseEvents = controllerEventLogRepository.GetSignalEventsByEventCodes(
                options.SignalIdentifier,
                options.Start,
                options.End,
                new List<int>() {
                    82,
                    WaitTimeOptions.PHASE_BEGIN_GREEN,
                    WaitTimeOptions.PHASE_CALL_DROPPED,
                    WaitTimeOptions.PHASE_END_RED_CLEARANCE,
                    WaitTimeOptions.PHASE_CALL_REGISTERED}
                );
            var terminationEvents = controllerEventLogs.Where(e =>
                new List<int> { 4, 5, 6, 7 }.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            var splitsEventCodes = new List<int>();
            for (var i = 130; i <= 151; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = controllerEventLogs.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            var volume = new VolumeCollection(
                options.Start,
                options.End,
                phaseEvents.Where(e => e.EventCode == 82).ToList(),
                options.BinSize);
            var analysisPhaseDataCollection = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                signal.SignalIdentifier,
                options.Start,
                options.End,
                planEvents,
                phaseEvents,
                splitsEvents,
                null,
                terminationEvents,
                signal,
                1);
            var tasks = new List<Task<WaitTimeResult>>();
            foreach (var approach in signal.Approaches)
            {
                tasks.Add(waitTimeService.GetChartData(
                options,
                approach,
                phaseEvents,
                analysisPhaseDataCollection.AnalysisPhases.Where(a => a.PhaseNumber == approach.ProtectedPhaseNumber).First(),
                analysisPhaseDataCollection.Plans,
                volume,
                approach.ProtectedPhaseNumber
                ));
            }
            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null);

        }
    }
}
