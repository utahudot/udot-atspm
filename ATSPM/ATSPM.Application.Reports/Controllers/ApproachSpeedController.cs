using ATSPM.Application.Reports.Business.ApproachSpeed;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachSpeedController : ControllerBase
    {
        private readonly ApproachSpeedService approachSpeedService;

        public ApproachSpeedController(ApproachSpeedService approachSpeedService)
        {
            this.approachSpeedService = approachSpeedService;
        }

        [HttpGet("test")]
        public ApproachSpeedResult Test()
        {
            Fixture fixture = new();
            ApproachSpeedResult viewModel = fixture.Create<ApproachSpeedResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public ApproachSpeedResult GetChartData([FromBody] ApproachSpeedOptions options)
        {
            ApproachSpeedResult viewModel = approachSpeedService.GetChartData(options);
            return viewModel;
        }
    }
}
