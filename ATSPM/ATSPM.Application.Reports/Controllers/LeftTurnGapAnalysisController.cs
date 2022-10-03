using ATSPM.Application.Reports.ViewModels.LeftTurnGapAnalysis;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeftTurnGapAnalysisController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public LeftTurnGapAnalyisiChart Test()
        {
            Fixture fixture = new();
            LeftTurnGapAnalyisiChart viewModel = fixture.Create<LeftTurnGapAnalyisiChart>();
            return viewModel;
        }

    }
}
