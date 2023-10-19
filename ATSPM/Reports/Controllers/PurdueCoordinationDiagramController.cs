using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using Reports.Business.Common;
using Reports.Business.PurdueCoordinationDiagram;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurdueCoordinationDiagramController : ControllerBase
    {
        private readonly PurdueCoordinationDiagramService perdueCoordinationDiagramService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly SignalPhaseService signalPhaseService;
        private readonly ISignalRepository signalRepository;
        private readonly PhaseService phaseService;

        public PurdueCoordinationDiagramController(
            PurdueCoordinationDiagramService perdueCoordinationDiagramService,
            IControllerEventLogRepository controllerEventLogRepository,
            SignalPhaseService signalPhaseService,
            ISignalRepository signalRepository,
            PhaseService phaseService)
        {
            this.perdueCoordinationDiagramService = perdueCoordinationDiagramService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalPhaseService = signalPhaseService;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PurdueCoordinationDiagramResult Test()
        {
            Fixture fixture = new();
            PurdueCoordinationDiagramResult viewModel = fixture.Create<PurdueCoordinationDiagramResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public async Task<IActionResult> GetChartData([FromBody] PurdueCoordinationDiagramOptions options)
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

            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<PurdueCoordinationDiagramResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(options, phase, controllerEventLogs, planEvents));
            }

            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).ToList();

            if (finalResultcheck.IsNullOrEmpty())
            {
                return Ok("No chart data found");
            }
            return Ok(finalResultcheck);
        }

        private async Task<PurdueCoordinationDiagramResult> GetChartDataForApproach(
            PurdueCoordinationDiagramOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<ControllerEventLog> controllerEventLogs,
            IReadOnlyList<ControllerEventLog> planEvents)
        {
            var signalPhase = await signalPhaseService.GetSignalPhaseData(
                phaseDetail,
                options.Start,
                options.End,
                options.SelectedBinSize,
                null,
                controllerEventLogs.ToList(),
                planEvents.ToList(),
                options.ShowVolumes);
            if (signalPhase == null)
            {
                return null;
            }
            PurdueCoordinationDiagramResult viewModel = perdueCoordinationDiagramService.GetChartData(options, phaseDetail.Approach, signalPhase);
            viewModel.SignalDescription = phaseDetail.Approach.Signal.SignalDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
