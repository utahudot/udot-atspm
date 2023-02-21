//using ATSPM.Application.Reports.Business.ArrivalOnRed;
//using ATSPM.Application.Reports.Business.LeftTurnGapAnalysis;
//using AutoFixture;
//using Microsoft.AspNetCore.Mvc;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ATSPM.Application.Reports.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class LeftTurnGapAnalysisController : ControllerBase
//    {
//        private readonly LeftTurnGapAnalysisService leftTurnGapAnalysisService;

//        //public LeftTurnGapAnalysisController(LeftTurnGapAnalysisService leftTurnGapAnalysisService)
//        //{
//        //    this.leftTurnGapAnalysisService = leftTurnGapAnalysisService;
//        //}

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public LeftTurnGapAnalysisResult Test()
//        {
//            Fixture fixture = new();
//            LeftTurnGapAnalysisResult viewModel = fixture.Create<LeftTurnGapAnalysisResult>();
//            return viewModel;
//        }

//        //[HttpPost("getChartData")]
//        //public LeftTurnGapAnalysisResult GetChartData([FromBody] LeftTurnGapAnalysisOptions options)
//        //{
//        //    LeftTurnGapAnalysisResult viewModel = leftTurnGapAnalysisService.GetChartData(options);
//        //    return viewModel;
//        //}

//    }
//}
