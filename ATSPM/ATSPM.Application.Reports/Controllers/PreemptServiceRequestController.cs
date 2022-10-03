using ATSPM.Application.Reports.ViewModels.PreemptServiceRequest;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreemptServiceRequestController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PreemptServiceRequestChart Test()
        {
            Fixture fixture = new();
            PreemptServiceRequestChart viewModel = fixture.Create<PreemptServiceRequestChart>();
            return viewModel;
        }

    }
}
