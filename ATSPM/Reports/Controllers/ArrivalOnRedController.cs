using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.ArrivalOnRed;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public ArrivalOnRedController(
            ArrivalOnRedService arrivalOnRedService,
            SignalPhaseService signalPhaseService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository
            )
        {
            this.arrivalOnRedService = arrivalOnRedService;
            this.signalPhaseService = signalPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
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
            var tasks = new List<Task<ArrivalOnRedResult>>();
            foreach (var approach in signal.Approaches)
            {
                tasks.Add(
                   GetChartDataByApproach(options, approach, controllerEventLogs, planEvents, signal.SignalDescription())
                );
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null);
        }

        private async Task<ArrivalOnRedResult> GetChartDataByApproach(
            ArrivalOnRedOptions options,
            Approach approach,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            string signalDescription)
        {
            var signalPhase = await signalPhaseService.GetSignalPhaseData(
                options.UsePermissivePhase,
                options.Start,
                options.End,
                options.SelectedBinSize,
                null,
                approach,
                controllerEventLogs,
                planEvents
                );
            if (signalPhase == null)
            {
                return null;
            }
            ArrivalOnRedResult viewModel = arrivalOnRedService.GetChartData(options, signalPhase, approach);
            viewModel.SignalDescription = signalDescription;
            return viewModel;
        }
    }
}
