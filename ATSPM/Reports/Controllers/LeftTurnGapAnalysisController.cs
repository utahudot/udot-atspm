//using ATSPM.Application.Extensions;
//using ATSPM.Application.Reports.Business.ArrivalOnRed;
//using ATSPM.Application.Reports.Business.LeftTurnGapAnalysis;
//using ATSPM.Application.Repositories;
//using AutoFixture;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Linq;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ATSPM.Application.Reports.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class LeftTurnGapAnalysisController : ControllerBase
//    {
//        private readonly LeftTurnGapAnalysisService leftTurnGapAnalysisService;
//        private readonly IApproachRepository approachRepository;
//        private readonly IControllerEventLogRepository controllerEventLogRepository;

//        public LeftTurnGapAnalysisController(
//            LeftTurnGapAnalysisService leftTurnGapAnalysisService,
//            IApproachRepository approachRepository,
//            IControllerEventLogRepository controllerEventLogRepository)
//        {
//            this.leftTurnGapAnalysisService = leftTurnGapAnalysisService;
//            this.approachRepository = approachRepository;
//            this.controllerEventLogRepository = controllerEventLogRepository;
//        }

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public LeftTurnGapAnalysisResult Test()
//        {
//            Fixture fixture = new();
//            LeftTurnGapAnalysisResult viewModel = fixture.Create<LeftTurnGapAnalysisResult>();
//            return viewModel;
//        }

//        [HttpPost("getChartData")]
//        public LeftTurnGapAnalysisResult GetChartData([FromBody] LeftTurnGapAnalysisOptions options)
//        {
//            var approach = approachRepository.GetList().Where(a => a.Id == options.ApproachId).FirstOrDefault();
//            var eventLogs = controllerEventLogRepository.GetSignalEventsByEventCodes(
//                options.SignalId,
//                options.StartDate,
//                options.EndDate,
//                new List<int> { 1, 10, 81 });
//            LeftTurnGapAnalysisResult viewModel = leftTurnGapAnalysisService.GetChartData(options, approach, eventLogs.ToList());
//            return viewModel;
//        }

//    }
//}
