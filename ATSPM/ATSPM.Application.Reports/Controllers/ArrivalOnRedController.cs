using ATSPM.Application.Reports.ViewModels.ArrivalOnRed;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArrivalOnRedController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public ArrivalOnRedChart Test()
        {
            Fixture fixture = new();
            ArrivalOnRedChart viewModel = fixture.Create<ArrivalOnRedChart>();
            return viewModel;
        }

    }
}
