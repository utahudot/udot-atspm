using ATSPM.Application.Reports.ViewModels.WaitTime;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaitTimeController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public WaitTimeChart Test()
        {
            Fixture fixture = new();
            WaitTimeChart viewModel = fixture.Create<WaitTimeChart>();
            return viewModel;
        }

    }
}
