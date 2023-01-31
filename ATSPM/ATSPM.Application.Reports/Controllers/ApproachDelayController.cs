using ATSPM.Application.Reports.Business.AppoachDelay;
using ATSPM.Application.Reports.ViewModels.ApproachDelay;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachDelayController : ControllerBase
    {
        private readonly ApproachDelayService approachDelayService;

        public ApproachDelayController(ApproachDelayService approachDelayService)
        {
            this.approachDelayService = approachDelayService;
        }

        [HttpGet("test")]
        public ApproachDelayResult Test()
        {
            Fixture fixture = new();
            ApproachDelayResult viewModel = fixture.Create<ApproachDelayResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public ApproachDelayResult GetChartData(
            ApproachDelayOptions options
            )
        {
            ApproachDelayResult viewModel = approachDelayService.GetChartData(options);
            return viewModel;
        }

        //[HttpPost("chart")]
        //public ApproachDelayChart Chart( )
        //{
        //  return ApproachService.GetApproachDelayChart()
        //}

    }
}
