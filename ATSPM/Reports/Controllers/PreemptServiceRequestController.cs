using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.PreemptService;
using ATSPM.Application.Reports.Business.PreemptServiceRequest;
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
    public class PreemptServiceRequestController : ControllerBase
    {
        private readonly PreemptServiceRequestService preemptServiceRequestService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public PreemptServiceRequestController(
            PreemptServiceRequestService preemptServiceRequestService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository)
        {
            this.preemptServiceRequestService = preemptServiceRequestService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PreemptServiceRequestResult Test()
        {
            Fixture fixture = new();
            PreemptServiceRequestResult viewModel = fixture.Create<PreemptServiceRequestResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public PreemptServiceRequestResult GetChartData([FromBody] PreemptServiceRequestOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);

            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(options.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var events = controllerEventLogs.GetEventsByEventCodes(options.Start, options.End, new List<int>() { 102 });
            PreemptServiceRequestResult viewModel = preemptServiceRequestService.GetChartData(options, planEvents, events);
            viewModel.SignalDescription = signal.SignalDescription();
            return viewModel;
        }

    }
}
