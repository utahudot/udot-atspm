using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.TimingAndActuation;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimingAndActuationController : ControllerBase
    {
        private readonly TimingAndActuationsForPhaseService timingAndActuationsForPhaseService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public TimingAndActuationController(
            TimingAndActuationsForPhaseService timingAndActuationsForPhaseService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository
            )
        {
            this.timingAndActuationsForPhaseService = timingAndActuationsForPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public TimingAndActuationsForPhaseResult Test()
        {
            Fixture fixture = new();
            TimingAndActuationsForPhaseResult viewModel = fixture.Create<TimingAndActuationsForPhaseResult>();
            return viewModel;
        }



        [HttpPost("getChartData")]
        public async Task<IEnumerable<TimingAndActuationsForPhaseResult>> GetChartData([FromBody] TimingAndActuationsOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var tasks = new List<Task<TimingAndActuationsForPhaseResult>>();

            foreach (var approach in signal.Approaches)
            {
                var eventCodes = new List<int> { };
                if (options.ShowAdvancedCount || options.ShowAdvancedDilemmaZone || options.ShowLaneByLaneCount || options.ShowStopBarPresence)
                    eventCodes.AddRange(new List<int> { 81, 82 });
                if (options.ShowPedestrianActuation)
                    eventCodes.AddRange(new List<int> { 89, 90 });
                if (options.ShowPedestrianIntervals)
                    eventCodes.AddRange(timingAndActuationsForPhaseService.GetPedestrianIntervalEventCodes(approach.IsPedestrianPhaseOverlap));
                if (options.PhaseEventCodesList != null)
                    eventCodes.AddRange(options.PhaseEventCodesList);
                tasks.Add(GetChartDataForPhase(options, controllerEventLogs, approach, eventCodes, false));
                if (approach.IsPermissivePhaseOverlap && approach.PermissivePhaseNumber.HasValue)
                    tasks.Add(GetChartDataForPhase(options, controllerEventLogs, approach, eventCodes, true));
            }
            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null);
        }

        private async Task<TimingAndActuationsForPhaseResult> GetChartDataForPhase(
            TimingAndActuationsOptions options,
            List<ControllerEventLog> controllerEventLogs,
            Approach approach,
            List<int> eventCodes,
            bool usePermissivePhase)
        {
            eventCodes.AddRange(timingAndActuationsForPhaseService.GetCycleCodes(
                (usePermissivePhase && approach.IsPermissivePhaseOverlap) ||
                (usePermissivePhase && approach.IsPermissivePhaseOverlap))
                );
            var approachevents = controllerEventLogs.GetEventsByEventCodes(
                options.Start,
                options.End,
                eventCodes,
                usePermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber).ToList();
            var viewModel = timingAndActuationsForPhaseService.GetChartData(options, approach, approachevents, usePermissivePhase);
            viewModel.SignalDescription = approach.Signal.SignalDescription();
            viewModel.ApproachDescription = approach.Description;
            return viewModel;
        }
    }
}
