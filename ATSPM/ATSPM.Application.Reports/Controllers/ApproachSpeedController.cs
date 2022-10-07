using ATSPM.Application.Reports.ViewModels.ApproachDelay;
using ATSPM.Application.Reports.ViewModels.ApproachSpeed;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachSpeedController : ControllerBase
    {
        [HttpGet("test")]
        public ApproachSpeedChart Test()
        {
            Fixture fixture = new();
            ApproachSpeedChart viewModel = fixture.Create<ApproachSpeedChart>();
            return viewModel;
        }

    }
}
