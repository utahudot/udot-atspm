using ATSPM.Application.Reports.ViewModels.TimingAndActuation;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimingAndActuationController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public TimingAndActuationResult Test()
        {
            Fixture fixture = new();
            TimingAndActuationResult viewModel = fixture.Create<TimingAndActuationResult>();
            return viewModel;
        }

    }
}
