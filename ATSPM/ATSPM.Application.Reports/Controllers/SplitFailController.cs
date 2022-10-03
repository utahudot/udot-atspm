using ATSPM.Application.Reports.ViewModels.SplitFail;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SplitFailController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public SplitFailChart Test()
        {
            Fixture fixture = new();
            SplitFailChart viewModel = fixture.Create<SplitFailChart>();
            return viewModel;
        }

    }
}
