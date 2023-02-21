//using ATSPM.Application.Reports.Business.LeftTurnGapAnalysis;
//using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
//using AutoFixture;
//using Microsoft.AspNetCore.Mvc;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ATSPM.Application.Reports.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PerdueCoordinationDiagramController : ControllerBase
//    {
//        private readonly PerdueCoordinationDiagramService perdueCoordinationDiagramService;

//        //public PerdueCoordinationDiagramController(PerdueCoordinationDiagramService perdueCoordinationDiagramService)
//        //{
//        //    this.perdueCoordinationDiagramService = perdueCoordinationDiagramService;
//        //}

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public PerdueCoordinationDiagramResult Test()
//        {
//            Fixture fixture = new();
//            PerdueCoordinationDiagramResult viewModel = fixture.Create<PerdueCoordinationDiagramResult>();
//            return viewModel;
//        }

//        //[HttpPost("getChartData")]
//        //public PerdueCoordinationDiagramResult GetChartData([FromBody] PerdueCoordinationDiagramOptions options)
//        //{
//        //    PerdueCoordinationDiagramResult viewModel = perdueCoordinationDiagramService.GetChartData(options);
//        //    return viewModel;
//        //}

//    }
//}
