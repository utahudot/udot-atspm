using ATSPM.Application.Reports.ViewModels.SplitMonitor;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SplitMonitorController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public SplitMonitorChart Test()
        {
            Fixture fixture = new();
            SplitMonitorChart viewModel = fixture.Create<SplitMonitorChart>();
            return viewModel;
        }

    }
}
