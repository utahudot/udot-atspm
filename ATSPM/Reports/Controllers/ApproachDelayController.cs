using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.AppoachDelay;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
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

        public ApproachDelayController(
            ApproachDelayService approachDelayService,
            SignalPhaseService signalPhaseService,
            ISignalRepository signalRepository,
            IControllerEventLogRepository controllerEventLogRepository
            )
        {
            this.approachDelayService = approachDelayService;
            this.signalPhaseService = signalPhaseService;
            this.signalRepository = signalRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
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

            var approachDelayResults = new List<ApproachDelayResult>();
            var tasks = new List<Task<ApproachDelayResult>>();
            foreach (var approach in signal.Approaches)
            {
                tasks.Add(
                    GetChartDataByApproach(options, approach, controllerEventLogs, planEvents, signal.SignalDescription())
                );
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null);
        }

        private async Task<ApproachDelayResult> GetChartDataByApproach(
            ApproachDelayOptions options,
            Approach approach,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            string signalDescription)
        {
            var detectorEvents = controllerEventLogs.GetDetectorEvents(8, approach, options.Start, options.End, true, false);
            if (detectorEvents == null)
            {
                return null;
            }

            var cycleEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(
                approach,
                options.GetPermissivePhase,
                options.Start,
                options.End);
            var signalPhase = await signalPhaseService.GetSignalPhaseData(
                options.Start,
                options.End,
                false,
                null,
                options.BinSize,
                approach,
                cycleEvents.ToList(),
                planEvents.ToList(),
                detectorEvents.ToList());
            ApproachDelayResult viewModel = approachDelayService.GetChartData(
                options,
                approach,
                signalPhase);
            viewModel.SignalDescription = signalDescription;
            viewModel.ApproachDescription = approach.Description;
            return viewModel;
        }


    }
}
