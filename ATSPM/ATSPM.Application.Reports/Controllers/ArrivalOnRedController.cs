using ATSPM.Application.Reports.Business.ArrivalOnRed;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArrivalOnRedController : ControllerBase
    {
        private readonly ArrivalOnRedService arrivalOnRedService;

        public ArrivalOnRedController(ArrivalOnRedService arrivalOnRedService)
        {
            this.arrivalOnRedService = arrivalOnRedService;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public ArrivalOnRedResult Test()
        {
            Fixture fixture = new();
            ArrivalOnRedResult viewModel = fixture.Create<ArrivalOnRedResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public ArrivalOnRedResult GetChartData([FromBody] ArrivalOnRedOptions options)
        {
            ArrivalOnRedResult viewModel = arrivalOnRedService.GetChartData(options);
            return viewModel;
        }

    }
}
