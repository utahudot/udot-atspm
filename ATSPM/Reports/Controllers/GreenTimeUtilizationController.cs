using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
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
    public class GreenTimeUtilizationController : ControllerBase
    {
        private readonly GreenTimeUtilizationService greenTimeUtilizationService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;
        private readonly PhaseService phaseService;

        public GreenTimeUtilizationController(
            GreenTimeUtilizationService GreenTimeUtilizationService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository,
            PhaseService phaseService)
        {
            this.greenTimeUtilizationService = GreenTimeUtilizationService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public GreenTimeUtilizationResult Test()
        {
            Fixture fixture = new();
            GreenTimeUtilizationResult viewModel = fixture.Create<GreenTimeUtilizationResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public async Task<IEnumerable<GreenTimeUtilizationResult>> GetChartData([FromBody] GreenTimeUtilizationOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<GreenTimeUtilizationResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(options, phase, controllerEventLogs, planEvents, false));
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null);
        }

        private async Task<GreenTimeUtilizationResult> GetChartDataForApproach(
            GreenTimeUtilizationOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<ControllerEventLog> controllerEventLogs,
            IReadOnlyList<ControllerEventLog> planEvents,
            bool usePermissivePhase)
        {
            var detectorEvents = controllerEventLogs.GetDetectorEvents(options.MetricTypeId, phaseDetail.Approach, options.Start, options.End, true, false).ToList();
            var cycleEvents = controllerEventLogs.GetEventsByEventCodes(options.Start, options.End, new List<int>() { 1, 8, 11 }).ToList();

            GreenTimeUtilizationResult viewModel = greenTimeUtilizationService.GetChartData(
                phaseDetail,
                options,
                detectorEvents,
                cycleEvents,
                planEvents.ToList(),
                controllerEventLogs.ToList()
                );
            viewModel.SignalDescription = phaseDetail.Approach.Signal.SignalDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
