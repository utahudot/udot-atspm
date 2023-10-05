using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.PreemptService;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreemptServiceController : ControllerBase
    {
        private readonly PreemptServiceService preemptServiceService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public PreemptServiceController(
            PreemptServiceService preemptServiceService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository
            )
        {
            this.preemptServiceService = preemptServiceService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PreemptServiceResult Test()
        {
            Fixture fixture = new();
            PreemptServiceResult viewModel = fixture.Create<PreemptServiceResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public PreemptServiceResult GetChartData([FromBody] PreemptServiceMetricOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(options.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var preemptEvents = controllerEventLogs.GetEventsByEventCodes(options.Start, options.End, new List<int>() { 105 });

            PreemptServiceResult viewModel = preemptServiceService.GetChartData(
                options,
                planEvents.ToList(),
                preemptEvents.ToList());
            viewModel.SignalDescription = signal.SignalDescription();
            return viewModel;
        }

    }
}
