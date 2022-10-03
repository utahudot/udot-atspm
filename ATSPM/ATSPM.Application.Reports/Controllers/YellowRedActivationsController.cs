using ATSPM.Application.Reports.ViewModels.YellowRedActivations;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YellowRedActivationsController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public YellowRedActivationsChart Test()
        {
            Fixture fixture = new();
            YellowRedActivationsChart viewModel = fixture.Create<YellowRedActivationsChart>();
            return viewModel;
        }

    }
}
