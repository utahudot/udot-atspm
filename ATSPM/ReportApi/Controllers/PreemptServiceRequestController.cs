using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.PreemptServiceRequest;
using ATSPM.ReportApi.TempExtensions;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.ReportApi.Controllers
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
        public IActionResult GetChartData([FromBody] PreemptServiceRequestOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            if (signal == null)
            {
                return BadRequest("Signal not found");
            }
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(options.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                return Ok("No Controller Event Logs found for signal");
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
                options.Start.AddHours(-12),
                options.End.AddHours(12)).ToList();
            var events = controllerEventLogs.GetEventsByEventCodes(options.Start, options.End, new List<int>() { 102 });
            PreemptServiceRequestResult viewModel = preemptServiceRequestService.GetChartData(options, planEvents, events);
            viewModel.SignalDescription = signal.SignalDescription();
            return Ok(viewModel);
        }

    }
}
