using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PedDelay;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Reports.Business.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedDelayController : ControllerBase
    {
        private readonly PedDelayService pedDelayService;
        private readonly PedPhaseService pedPhaseService;
        private readonly CycleService cycleService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;
        private readonly PhaseService phaseService;

        public PedDelayController(
            PedDelayService pedDelayService,
            PedPhaseService pedPhaseService,
            CycleService cycleService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository,
            PhaseService phaseService)
        {
            this.pedDelayService = pedDelayService;
            this.pedPhaseService = pedPhaseService;
            this.cycleService = cycleService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PedDelayResult Test()
        {
            Fixture fixture = new();
            PedDelayResult viewModel = fixture.Create<PedDelayResult>();
            return viewModel;
        }

        [HttpPost("GetChartData")]
        public async Task<IEnumerable<PedDelayResult>> GetChartData(
            PedDelayOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<PedDelayResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(options, phase, planEvents, controllerEventLogs));
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null);
        }

        private async Task<PedDelayResult> GetChartDataForApproach(
            PedDelayOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<ControllerEventLog>
            planEvents,
            IReadOnlyList<ControllerEventLog> events)
        {
            var cycleEvents = events.GetCycleEventsWithTimeExtension(phaseDetail.PhaseNumber, phaseDetail.UseOverlap, options.Start, options.End);
            var pedEvents = events.GetPedEvents(options.Start, options.End, phaseDetail.Approach);
            if (pedEvents.IsNullOrEmpty())
                return null;
            var pedPhaseData = pedPhaseService.GetPedPhaseData(
                options,
                phaseDetail.Approach,
                planEvents.ToList(),
                pedEvents.ToList());

            var cycles = cycleService.GetRedToRedCycles(
                options.Start,
                options.End,
                cycleEvents.ToList());

            PedDelayResult viewModel = pedDelayService.GetChartData(
                options,
                pedPhaseData,
                cycles
                );
            viewModel.SignalDescription = phaseDetail.Approach.Signal.SignalDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
