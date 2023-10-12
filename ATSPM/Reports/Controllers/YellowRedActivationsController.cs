using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.YellowRedActivations;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using IdentityServer4.Extensions;
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
    public class YellowRedActivationsController : ControllerBase
    {
        private readonly YellowRedActivationsService yellowRedActivationsService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;
        private readonly PhaseService phaseService;

        public YellowRedActivationsController(
            YellowRedActivationsService yellowRedActivationsService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository,
            PhaseService phaseService)
        {
            this.yellowRedActivationsService = yellowRedActivationsService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
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
        public async Task<IActionResult> GetChartData([FromBody] YellowRedActivationsOptions options)
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
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<YellowRedActivationsResult>>();
            foreach (var phaseDetail in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(options, phaseDetail, controllerEventLogs, planEvents, signal.SignalDescription()));
            }

            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).ToList();

            if (finalResultcheck.IsNullOrEmpty())
            {
                return Ok("No chart data found");
            }
            return Ok(finalResultcheck);
        }

        private async Task<YellowRedActivationsResult> GetChartDataForApproach(
            YellowRedActivationsOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            string signalDescription)
        {
            var cycleEvents = controllerEventLogs.GetEventsByEventCodes(
                options.Start.AddSeconds(-900),
                options.End.AddSeconds(900),
                GetYellowRedActivationsCycleEventCodes(phaseDetail.UseOverlap),
                phaseDetail.PhaseNumber)
                .OrderBy(e => e.Timestamp)
                .ToList();
            var detectorEvents = controllerEventLogRepository.GetDetectorEvents(
                options.MetricTypeId,
                phaseDetail.Approach,
                options.Start,
                options.End,
                true,
                false);

            var viewModel = yellowRedActivationsService.GetChartData(
                options,
                phaseDetail,
                cycleEvents,
                detectorEvents,
                planEvents);
            viewModel.SignalDescription = signalDescription;
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }

        private List<int> GetYellowRedActivationsCycleEventCodes(bool useOverlap)
        {
            return useOverlap
                ? new List<int> { 62, 63, 64 }
                : new List<int> { 1, 8, 9, 11 };
        }
    }
}
