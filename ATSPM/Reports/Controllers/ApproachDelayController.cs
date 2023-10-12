using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.AppoachDelay;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using Reports.Business.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachDelayController : ControllerBase
    {
        private readonly ApproachDelayService approachDelayService;
        private readonly SignalPhaseService signalPhaseService;
        private readonly ISignalRepository signalRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly PhaseService phaseService;

        public ApproachDelayController(
            ApproachDelayService approachDelayService,
            SignalPhaseService signalPhaseService,
            ISignalRepository signalRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            PhaseService phaseService
            )
        {
            this.approachDelayService = approachDelayService;
            this.signalPhaseService = signalPhaseService;
            this.signalRepository = signalRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.phaseService = phaseService;
        }

        [HttpGet("test")]
        public ApproachDelayResult Test()
        {
            Fixture fixture = new();
            ApproachDelayResult viewModel = fixture.Create<ApproachDelayResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public async Task<IActionResult> GetChartData([FromBody] ApproachDelayOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            if(signal == null)
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
            var tasks = new List<Task<ApproachDelayResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataByApproach(options, phase, controllerEventLogs, planEvents, signal.SignalDescription()));
            }

            var results = await Task.WhenAll(tasks);
            var finalResultcheck = results.Where(result => result != null).ToList();

            if (finalResultcheck.IsNullOrEmpty())
            {
                return Ok("No chart data found");
            }
            return Ok(finalResultcheck);
        }

        private async Task<ApproachDelayResult> GetChartDataByApproach(
            ApproachDelayOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            string signalDescription)
        {
            var signalPhase = await signalPhaseService.GetSignalPhaseData(
                phaseDetail,
                options.Start,
                options.End,
                options.BinSize,
                null,
                controllerEventLogs,
                planEvents,
                false);
            if (signalPhase == null)
            {
                return null;
            }
            ApproachDelayResult viewModel = approachDelayService.GetChartData(
                options,
                phaseDetail,
                signalPhase);
            viewModel.SignalDescription = signalDescription;
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }


    }
}
