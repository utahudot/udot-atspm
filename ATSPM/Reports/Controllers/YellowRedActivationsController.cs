//using ATSPM.Application.Extensions;
//using ATSPM.Application.Reports.Business.WaitTime;
//using ATSPM.Application.Reports.Business.YellowRedActivations;
//using ATSPM.Application.Repositories;
//using ATSPM.Data.Models;
//using AutoFixture;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ATSPM.Application.Reports.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class YellowRedActivationsController : ControllerBase
//    {
//        private readonly YellowRedActivationsService yellowRedActivationsService;
//        private readonly IApproachRepository approachRepository;
//        private readonly IControllerEventLogRepository controllerEventLogRepository;

//        public YellowRedActivationsController(
//            YellowRedActivationsService yellowRedActivationsService,
//            IApproachRepository approachRepository,
//            IControllerEventLogRepository controllerEventLogRepository)
//        {
//            this.yellowRedActivationsService = yellowRedActivationsService;
//            this.approachRepository = approachRepository;
//            this.controllerEventLogRepository = controllerEventLogRepository;
//        }

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public YellowRedActivationsResult Test()
//        {
//            Fixture fixture = new();
//            YellowRedActivationsResult viewModel = fixture.Create<YellowRedActivationsResult>();
//            return viewModel;
//        }

//        [HttpPost("getChartData")]
//        public YellowRedActivationsResult GetChartData([FromBody] YellowRedActivationsOptions options)
//        {            
//            var approach = approachRepository.Lookup(options.ApproachId);
//            var cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(
//                approach.SignalId,
//                options.Start.AddSeconds(-900),
//                options.End.AddSeconds(900),
//                GetYellowRedActivationsCycleEventCodes(approach, options.UsePermissivePhase),
//                options.UsePermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber);
//            var detectorEvents = controllerEventLogRepository.GetDetectorEvents(
//                11,
//                approach,
//                options.Start,
//                options.End,
//                true,
//                false);
//            var planEvents = controllerEventLogRepository.GetPlanEvents(
//                approach.SignalId,
//                options.Start,
//                options.End);

//            return yellowRedActivationsService.GetChartData(
//                options, 
//                approach, 
//                cycleEvents,
//                detectorEvents,
//                planEvents);
//        }

//        public List<int> GetYellowRedActivationsCycleEventCodes(Approach approach, bool getPermissivePhase)
//        {
//            return (getPermissivePhase && approach.IsPermissivePhaseOverlap) || (!getPermissivePhase && approach.IsProtectedPhaseOverlap)
//                ? new List<int> { 62, 63, 64 }
//                : new List<int> { 1, 8, 9, 10, 11 };
//        }
//    }
//}
