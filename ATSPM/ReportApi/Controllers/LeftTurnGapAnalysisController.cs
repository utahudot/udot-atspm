using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.LeftTurnGapAnalysis;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.ReportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeftTurnGapAnalysisController : ControllerBase
    {
        private readonly LeftTurnGapAnalysisService leftTurnGapAnalysisService;
        private readonly IApproachRepository approachRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public LeftTurnGapAnalysisController(
            LeftTurnGapAnalysisService leftTurnGapAnalysisService,
            IApproachRepository approachRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository)
        {
            this.leftTurnGapAnalysisService = leftTurnGapAnalysisService;
            this.approachRepository = approachRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public LeftTurnGapAnalysisResult Test()
        {
            Fixture fixture = new();
            LeftTurnGapAnalysisResult viewModel = fixture.Create<LeftTurnGapAnalysisResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public async Task<IActionResult> GetChartData([FromBody] LeftTurnGapAnalysisOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            if (signal == null)
            {
                return BadRequest("Signal not found");
            }
            var eventCodes = new List<int> { 1, 10, 81 };
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(
                signal.SignalIdentifier,
                options.Start,
                options.End)
                .ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                return Ok("No Controller Event Logs found for signal");
            }

            var tasks = new List<Task<LeftTurnGapAnalysisResult>>();
            var leftTurnGapData = new List<LeftTurnGapAnalysisResult>();
            //Get phase + check for opposing phase before creating chart
            var ebPhase = signal.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 6);
            if (ebPhase != null && signal.Approaches.Any(x => x.ProtectedPhaseNumber == 2))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(ebPhase, controllerEventLogs, options));
            }

            var nbPhase = signal.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 8);
            if (nbPhase != null && signal.Approaches.Any(x => x.ProtectedPhaseNumber == 4))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(nbPhase, controllerEventLogs, options));
            }

            var wbPhase = signal.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 2);
            if (wbPhase != null && signal.Approaches.Any(x => x.ProtectedPhaseNumber == 6))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(wbPhase, controllerEventLogs, options));
            }

            var sbPhase = signal.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 4);
            if (sbPhase != null && signal.Approaches.Any(x => x.ProtectedPhaseNumber == 8))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(sbPhase, controllerEventLogs, options));
            }
            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).ToList();

            if (finalResultcheck.IsNullOrEmpty())
            {
                return Ok("No chart data found");
            }
            return Ok(finalResultcheck);
        }


    }
}
