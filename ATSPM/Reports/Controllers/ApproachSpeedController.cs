using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.ApproachSpeed;
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
    public class ApproachSpeedController : ControllerBase
    {
        private readonly ApproachSpeedService approachSpeedService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly IApproachRepository approachRepository;
        private readonly ISpeedEventRepository speedEventRepository;
        private readonly ISignalRepository signalRepository;

        public ApproachSpeedController(
            ApproachSpeedService approachSpeedService,
            IControllerEventLogRepository controllerEventLogRepository,
            IApproachRepository approachRepository,
            ISpeedEventRepository speedEventRepository,
            ISignalRepository signalRepository)
        {
            this.approachSpeedService = approachSpeedService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.approachRepository = approachRepository;
            this.speedEventRepository = speedEventRepository;
            this.signalRepository = signalRepository;
        }

        [HttpGet("test")]
        public ApproachSpeedResult Test()
        {
            Fixture fixture = new();
            ApproachSpeedResult viewModel = fixture.Create<ApproachSpeedResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public async Task<IEnumerable<ApproachSpeedResult>> GetChartData([FromBody] ApproachSpeedOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var tasks = new List<Task<ApproachSpeedResult>>();
            foreach (var approach in signal.Approaches)
            {
                tasks.Add(GetChartDataByApproach(options, controllerEventLogs, planEvents, approach, signal.SignalDescription()));
            }
            var results = await Task.WhenAll(tasks);
            return results;

        }

        private async Task<ApproachSpeedResult> GetChartDataByApproach(
            ApproachSpeedOptions options,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            Approach approach,
            string signalDescription)
        {
            var detector = approach.GetDetectorsForMetricType(options.MetricTypeId).First();
            var speedEvents = speedEventRepository.GetSpeedEventsByDetector(
                detector,
                options.Start,
                options.End,
                detector.MinSpeedFilter ?? 5).ToList();
            var cycleEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(
                approach,
                options.GetPermissivePhase,
                options.Start,
                options.End);
            ApproachSpeedResult viewModel = approachSpeedService.GetChartData(
                options,
                cycleEvents.ToList(),
                planEvents,
                speedEvents,
                detector);
            viewModel.SignalDescription = signalDescription;
            viewModel.ApproachDescription = approach.Description;
            return viewModel;
        }
    }
}
