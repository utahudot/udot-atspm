using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.PreempDetail;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreemptDetailController : ControllerBase
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly PreemptDetailService preemptDetailService;
        private readonly ISignalRepository signalRepository;

        public PreemptDetailController(
            IControllerEventLogRepository controllerEventLogRepository,
            PreemptDetailService preemptDetailService,
            ISignalRepository signalRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.preemptDetailService = preemptDetailService;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PreemptDetailResult Test()
        {
            Fixture fixture = new();
            PreemptDetailResult viewModel = fixture.Create<PreemptDetailResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public IActionResult GetChartData([FromBody] PreemptDetailOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            if (signal == null)
            {
                return BadRequest("Signal not found");
            }
            var codes = new List<int>();

            for (var i = 101; i <= 111; i++)
                codes.Add(i);

            var events = controllerEventLogRepository.GetSignalEventsByEventCodes(
                options.SignalIdentifier,
                options.Start,
                options.End,
                codes).ToList();

            if (events.IsNullOrEmpty())
            {
                return Ok("No Controller Event Logs found for signal");
            }


            PreemptDetailResult viewModel = preemptDetailService.GetChartData(options, events);
            viewModel.SignalDescription = signal.SignalDescription();
            return Ok(viewModel);
        }

    }
}
