using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.YellowRedActivations;
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
    public class YellowRedActivationsController : ControllerBase
    {
        private readonly YellowRedActivationsService yellowRedActivationsService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public YellowRedActivationsController(
            YellowRedActivationsService yellowRedActivationsService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository)
        {
            this.yellowRedActivationsService = yellowRedActivationsService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public YellowRedActivationsResult Test()
        {
            Fixture fixture = new();
            YellowRedActivationsResult viewModel = fixture.Create<YellowRedActivationsResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public async Task<IEnumerable<YellowRedActivationsResult>> GetChartData([FromBody] YellowRedActivationsOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var tasks = new List<Task<YellowRedActivationsResult>>();
            foreach (var approach in signal.Approaches)
            {
                tasks.Add(GetChartDataForApproach(options, approach, controllerEventLogs, planEvents, signal.SignalDescription()));
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null);
        }

        private async Task<YellowRedActivationsResult> GetChartDataForApproach(
            YellowRedActivationsOptions options,
            Approach approach,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            string signalDescription)
        {
            var cycleEvents = controllerEventLogs.GetEventsByEventCodes(
                options.Start.AddSeconds(-900),
                options.End.AddSeconds(900),
                GetYellowRedActivationsCycleEventCodes(approach, false),
                approach.ProtectedPhaseNumber).OrderBy(e => e.Timestamp).ToList();
            var detectorEvents = controllerEventLogRepository.GetDetectorEvents(
                options.MetricTypeId,
                approach,
                options.Start,
                options.End,
                true,
                false);

            var viewModel = yellowRedActivationsService.GetChartData(
                options,
                approach,
                cycleEvents,
                detectorEvents,
                planEvents);
            viewModel.SignalDescription = signalDescription;
            viewModel.ApproachDescription = approach.Description;
            return viewModel;
        }

        private List<int> GetYellowRedActivationsCycleEventCodes(Approach approach, bool getPermissivePhase)
        {
            return (getPermissivePhase && approach.IsPermissivePhaseOverlap) || (!getPermissivePhase && approach.IsProtectedPhaseOverlap)
                ? new List<int> { 62, 63, 64 }
                : new List<int> { 1, 8, 9, 11 };
        }
    }
}
