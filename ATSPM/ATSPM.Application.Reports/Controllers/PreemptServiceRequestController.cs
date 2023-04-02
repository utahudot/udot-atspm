using ATSPM.Application.Reports.Business.PreemptService;
using ATSPM.Application.Reports.Business.PreemptServiceRequest;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreemptServiceRequestController : ControllerBase
    {
        private readonly PreemptServiceRequestService preemptServiceRequestService;

        public PreemptServiceRequestController(PreemptServiceRequestService preemptServiceRequestService)
        {
            this.preemptServiceRequestService = preemptServiceRequestService;
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
            PreemptServiceRequestResult viewModel = preemptServiceRequestService.GetChartData(options);
            return viewModel;
        }

    }
}
