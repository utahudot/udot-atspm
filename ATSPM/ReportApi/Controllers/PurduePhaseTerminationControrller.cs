using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.PhaseTermination;
using ATSPM.ReportApi.TempExtensions;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.ReportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurduePhaseTerminationController : ControllerBase
    {
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public PurduePhaseTerminationController(
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository)
        {
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PhaseTerminationResult Test()
        {
            Fixture fixture = new();
            PhaseTerminationResult viewModel = fixture.Create<PhaseTerminationResult>();
            return viewModel;
        }



        [HttpPost("getChartData")]
        public IActionResult GetChartData([FromBody] PurduePhaseTerminationOptions options)
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
            var terminationEvents = controllerEventLogs.Where(e =>
                new List<int> { 4, 5, 6, 7 }.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            var pedEvents = controllerEventLogs.Where(e =>
                new List<int> { 21, 23 }.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            var cycleEvents = controllerEventLogs.Where(e =>
                new List<int> { 1, 11 }.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            var splitsEventCodes = new List<int>();
            for (var i = 130; i <= 151; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = controllerEventLogs.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            controllerEventLogs = null;
            GC.Collect();

            var phaseCollectionData = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                options.SignalIdentifier,
                options.Start,
                options.End,
                planEvents,
                cycleEvents,
                splitsEvents,
                pedEvents,
                terminationEvents,
                signal,
                options.SelectedConsecutiveCount);
            var phases = new List<Phase>();
            foreach (var phase in phaseCollectionData.AnalysisPhases)
            {
                phases.Add(new Phase(
                    phase.PhaseNumber,
                    phase.ConsecutiveGapOuts.Select(g => g.Timestamp).ToList(),
                    phase.ConsecutiveMaxOut.Select(g => g.Timestamp).ToList(),
                    phase.ConsecutiveForceOff.Select(g => g.Timestamp).ToList(),
                    phase.PedestrianEvents.Select(g => g.Timestamp).ToList(),
                    phase.UnknownTermination.Select(g => g.Timestamp).ToList()
                    ));
            }

            var plans = phaseCollectionData.Plans.Select(p => new Plan(p.PlanNumber.ToString(), p.Start, p.End)).ToList();
            var result = new PhaseTerminationResult(
                phaseCollectionData.SignalId,
                options.Start,
                options.End,
                options.SelectedConsecutiveCount,
                plans,
                phases
                );
            result.SignalDescription = signal.SignalDescription();
            return Ok(result);
        }
    }
}
