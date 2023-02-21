//using ATSPM.Application.Reports.Business.PreempDetail;
//using ATSPM.Application.Reports.Business.PreemptService;
//using AutoFixture;
//using Microsoft.AspNetCore.Mvc;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ATSPM.Application.Reports.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PreemptServiceController : ControllerBase
//    {
//        private readonly PreemptServiceService preemptServiceService;

//        //public PreemptServiceController(PreemptServiceService preemptServiceService)
//        //{
//        //    this.preemptServiceService = preemptServiceService;
//        //}

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public PreemptServiceResult Test()
//        {
//            Fixture fixture = new();
//            PreemptServiceResult viewModel = fixture.Create<PreemptServiceResult>();
//            return viewModel;
//        }

//        //[HttpPost("getChartData")]
//        //public PreemptServiceResult GetChartData([FromBody] PreemptServiceMetricOptions options)
//        //{
//        //    PreemptServiceResult viewModel = preemptServiceService.GetChartData(options);
//        //    return viewModel;
//        //}

//    }
//}
