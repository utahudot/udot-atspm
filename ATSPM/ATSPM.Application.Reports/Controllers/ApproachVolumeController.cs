//using ATSPM.Application.Reports.Business.ApproachSpeed;
//using ATSPM.Application.Reports.ViewModels.ApproachVolume;
//using ATSPM.Application.Reports.Business.ApproachVolume;
//using AutoFixture;
//using Microsoft.AspNetCore.Mvc;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ATSPM.Application.Reports.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ApproachVolumeController : ControllerBase
//    {
//        private readonly ApproachVolumeService approachVolumeService;

//        //public ApproachVolumeController(ApproachVolumeService approachVolumeService)
//        //{
//        //    this.approachVolumeService = approachVolumeService;
//        //}

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public ApproachVolumeResult Test()
//        {
//            Fixture fixture = new();
//            ApproachVolumeResult approachVolumeViewModel = fixture.Create<ApproachVolumeResult>();
//            return approachVolumeViewModel;
//        }


//        //[HttpPost("getChartData")]
//        //public ApproachVolumeResult GetChartData([FromBody] ApproachVolumeOptions options)
//        //{
//        //    ApproachVolumeResult viewModel = approachVolumeService.GetChartData(options);
//        //    return viewModel;
//        //}

//    }
//}
