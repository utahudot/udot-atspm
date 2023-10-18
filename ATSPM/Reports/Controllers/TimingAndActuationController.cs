using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.TimingAndActuation;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using Reports.Business.Common;
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
        private readonly PhaseService phaseService;

        public TimingAndActuationController(
            TimingAndActuationsForPhaseService timingAndActuationsForPhaseService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository,
            PhaseService phaseService
            )
        {
            this.timingAndActuationsForPhaseService = timingAndActuationsForPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
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
        public async Task<IActionResult> GetChartData([FromBody] TimingAndActuationsOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            if (signal == null)
            {
                return BadRequest("Signal not found");
            }
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                return Ok("No Controller Event Logs found for signal");
            }

            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<TimingAndActuationsForPhaseResult>>();

            foreach (var phase in phaseDetails)
            {
                var eventCodes = new List<int> { };
                if (options.ShowAdvancedCount || options.ShowAdvancedDilemmaZone || options.ShowLaneByLaneCount || options.ShowStopBarPresence)
                    eventCodes.AddRange(new List<int> { 81, 82 });
                if (options.ShowPedestrianActuation)
                    eventCodes.AddRange(new List<int> { 89, 90 });
                if (options.ShowPedestrianIntervals)
                    eventCodes.AddRange(timingAndActuationsForPhaseService.GetPedestrianIntervalEventCodes(phase.Approach.IsPedestrianPhaseOverlap));
                if (options.PhaseEventCodesList != null)
                    eventCodes.AddRange(options.PhaseEventCodesList);
                tasks.Add(GetChartDataForPhase(options, controllerEventLogs, phase, eventCodes, false));
            }
            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).ToList();

            if (finalResultcheck.IsNullOrEmpty())
            {
                return Ok("No chart data found");
            }
            return Ok(finalResultcheck);
        }

        private async Task<TimingAndActuationsForPhaseResult> GetChartDataForPhase(
            TimingAndActuationsOptions options,
            List<ControllerEventLog> controllerEventLogs,
            PhaseDetail phaseDetail,
            List<int> eventCodes,
            bool usePermissivePhase)
        {
            eventCodes.AddRange(timingAndActuationsForPhaseService.GetCycleCodes(phaseDetail.UseOverlap));
            var approachevents = controllerEventLogs.GetEventsByEventCodes(
                options.Start,
                options.End,
                eventCodes).ToList();
            var viewModel = timingAndActuationsForPhaseService.GetChartData(options, phaseDetail, approachevents, usePermissivePhase);
            viewModel.SignalDescription = phaseDetail.Approach.Signal.SignalDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
