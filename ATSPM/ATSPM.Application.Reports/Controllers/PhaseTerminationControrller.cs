using ATSPM.Application.Reports.ViewModels.PhaseTerminationChart;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhaseTerminationController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PhaseTerminationChart Test()
        {
            Fixture fixture = new();
            PhaseTerminationChart viewModel = fixture.Create<PhaseTerminationChart>();
            return viewModel;
        }

    }
}
