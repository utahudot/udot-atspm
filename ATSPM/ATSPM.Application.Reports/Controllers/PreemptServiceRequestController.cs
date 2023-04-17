using ATSPM.Application.Reports.Business.PreemptService;
using ATSPM.Application.Reports.Business.PreemptServiceRequest;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
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

        public PreemptServiceRequestController(
            PreemptServiceRequestService preemptServiceRequestService,
            IControllerEventLogRepository controllerEventLogRepository)
        {
            this.preemptServiceRequestService = preemptServiceRequestService;
            this.controllerEventLogRepository = controllerEventLogRepository;
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
            var planEvents = controllerEventLogRepository.GetPlanEvents(
                options.SignalId,
                options.Start,
                options.End);
            PreemptServiceRequestResult viewModel = preemptServiceRequestService.GetChartData(options, planEvents.ToList());
            return viewModel;
        }

    }
}
