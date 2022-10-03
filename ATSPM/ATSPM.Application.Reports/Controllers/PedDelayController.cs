using ATSPM.Application.Reports.ViewModels.PedDelay;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedDelayController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PedDelayChart Test()
        {
            Fixture fixture = new();
            PedDelayChart viewModel = fixture.Create<PedDelayChart>();
            return viewModel;
        }

    }
}
