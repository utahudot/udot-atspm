using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
using ATSPM.Application.Reports.Business.PreempDetail;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreemptDetailController : ControllerBase
    {
        private readonly PreemptDetailService preemptDetailService;

        public PreemptDetailController(PreemptDetailService preemptDetailService)
        {
            this.preemptDetailService = preemptDetailService;
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
        public PreemptDetailResult GetChartData([FromBody] PerdueCoordinationDiagramOptions options)
        {
            PreemptDetailResult viewModel = preemptDetailService.GetChartData(options);
            return viewModel;
        }

    }
}
