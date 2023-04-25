using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.PreemptService;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.Infrastructure.Repositories;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
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

        public PreemptServiceController(
            PreemptServiceService preemptServiceService,
            IControllerEventLogRepository controllerEventLogRepository
            )
        {
            this.preemptServiceService = preemptServiceService;
            this.controllerEventLogRepository = controllerEventLogRepository;
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
            var preemptEvents = controllerEventLogRepository.GetSignalEventsByEventCode(options.SignalId, options.Start, options.End, 105);
            var planEvents = controllerEventLogRepository.GetPlanEvents(
                options.SignalId,
                options.Start,
                options.End);
            PreemptServiceResult viewModel = preemptServiceService.GetChartData(
                options,
                planEvents.ToList(),
                preemptEvents.ToList());
            return viewModel;
        }

    }
}
