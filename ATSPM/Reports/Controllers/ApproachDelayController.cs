using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.AppoachDelay;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
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
        public async Task<IEnumerable<ApproachDelayResult>> GetChartData([FromBody] ApproachDelayOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
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

            return results.Where(result => result != null);
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
