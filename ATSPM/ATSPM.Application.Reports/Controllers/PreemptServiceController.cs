using ATSPM.Application.Reports.ViewModels.PreemptService;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreemptServiceController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PreemptServiceResult Test()
        {
            Fixture fixture = new();
            PreemptServiceResult viewModel = fixture.Create<PreemptServiceResult>();
            return viewModel;
        }

    }
}
