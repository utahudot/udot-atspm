using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
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

        public PurdueCoordinationDiagramController(
            PurdueCoordinationDiagramService perdueCoordinationDiagramService,
            IControllerEventLogRepository controllerEventLogRepository,
            SignalPhaseService signalPhaseService,
            ISignalRepository signalRepository)
        {
            this.perdueCoordinationDiagramService = perdueCoordinationDiagramService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalPhaseService = signalPhaseService;
            this.signalRepository = signalRepository;
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
        public async Task<IEnumerable<PurdueCoordinationDiagramResult>> GetChartData([FromBody] PurdueCoordinationDiagramOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var tasks = new List<Task<PurdueCoordinationDiagramResult>>();
            foreach (var approach in signal.Approaches)
            {
                tasks.Add(GetChartDataForApproach(options, approach, controllerEventLogs, planEvents));
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null);
        }

        private async Task<PurdueCoordinationDiagramResult> GetChartDataForApproach(PurdueCoordinationDiagramOptions options, Approach approach, IReadOnlyList<ControllerEventLog> controllerEventLogs, IReadOnlyList<ControllerEventLog> planEvents)
        {
            var signalPhase = await signalPhaseService.GetSignalPhaseData(
                options.UsePermissivePhase,
                options.Start,
                options.End,
                options.SelectedBinSize,
                null,
                approach,
                controllerEventLogs.ToList(),
                planEvents.ToList(),
                options.ShowVolumes);
            if (signalPhase == null)
            {
                return null;
            }
            PurdueCoordinationDiagramResult viewModel = perdueCoordinationDiagramService.GetChartData(options, approach, signalPhase);
            viewModel.SignalDescription = approach.Signal.SignalDescription();
            viewModel.ApproachDescription = approach.Description;
            return viewModel;
        }
    }
}
