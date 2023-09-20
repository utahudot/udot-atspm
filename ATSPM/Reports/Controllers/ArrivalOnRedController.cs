using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.ArrivalOnRed;
using ATSPM.Application.Reports.Business.Common;
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
    public class ArrivalOnRedController : ControllerBase
    {
        private readonly ArrivalOnRedService arrivalOnRedService;
        private readonly SignalPhaseService signalPhaseService;
        private readonly ISignalRepository signalRepository;
        private readonly PhaseService phaseService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public ArrivalOnRedController(
            ArrivalOnRedService arrivalOnRedService,
            SignalPhaseService signalPhaseService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository,
            PhaseService phaseService
            )
        {
            this.arrivalOnRedService = arrivalOnRedService;
            this.signalPhaseService = signalPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public ArrivalOnRedResult Test()
        {
            Fixture fixture = new();
            ArrivalOnRedResult viewModel = fixture.Create<ArrivalOnRedResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public async Task<IEnumerable<ArrivalOnRedResult>> GetChartData([FromBody] ArrivalOnRedOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<ArrivalOnRedResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(
                   GetChartDataByApproach(options, phase, controllerEventLogs, planEvents, signal.SignalDescription())
                );
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null);
        }

        private async Task<ArrivalOnRedResult> GetChartDataByApproach(
            ArrivalOnRedOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            string signalDescription)
        {
            var signalPhase = await signalPhaseService.GetSignalPhaseData(
                phaseDetail,
                options.Start,
                options.End,
                options.SelectedBinSize,
                null,
                controllerEventLogs,
                planEvents,
                false
                );
            if (signalPhase == null)
            {
                return null;
            }
            ArrivalOnRedResult viewModel = arrivalOnRedService.GetChartData(options, signalPhase, phaseDetail.Approach);
            viewModel.SignalDescription = signalDescription;
            return viewModel;
        }
    }
}
