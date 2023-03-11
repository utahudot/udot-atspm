using ATSPM.Application.Reports.Business.PreemptService;
using ATSPM.Application.Reports.Business.PreemptServiceRequest;
using ATSPM.Application.Reports.Business.SplitMonitor;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SplitMonitorController : ControllerBase
    {
        private readonly SplitMonitorService splitMonitorService;

        public SplitMonitorController(SplitMonitorService splitMonitorService)
        {
            this.splitMonitorService = splitMonitorService;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public SplitMonitorResult Test()
        {
            Fixture fixture = new();
            SplitMonitorResult viewModel = fixture.Create<SplitMonitorResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public SplitMonitorResult GetChartData([FromBody] SplitMonitorOptions options)
        {
            PreemptServiceRequestResult viewModel = splitMonitorService.GetChartData(options);
            return viewModel;
        }

    }
}
