using ATSPM.Application.Reports.ViewModels.TurningMovementCounts;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurningMovementCountsController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public TurningMovementCountChart Test()
        {
            Fixture fixture = new();
            TurningMovementCountChart viewModel = fixture.Create<TurningMovementCountChart>();
            return viewModel;
        }

    }
}
