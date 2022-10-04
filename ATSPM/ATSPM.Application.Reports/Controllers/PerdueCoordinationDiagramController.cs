using ATSPM.Application.Reports.ViewModels.PerdueCoordinationDiagram;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerdueCoordinationDiagramController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PerdueCoordinationDiagramChart Test()
        {
            Fixture fixture = new();
            PerdueCoordinationDiagramChart viewModel = fixture.Create<PerdueCoordinationDiagramChart>();
            return viewModel;
        }

    }
}
