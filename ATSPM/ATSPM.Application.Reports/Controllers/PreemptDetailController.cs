using ATSPM.Application.Reports.ViewModels.PreemptDetail;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreemptDetailController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PreemptDetailChart Test()
        {
            Fixture fixture = new();
            PreemptDetailChart viewModel = fixture.Create<PreemptDetailChart>();
            return viewModel;
        }

    }
}
