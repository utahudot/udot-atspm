//using ATSPM.Application.Extensions;
//using ATSPM.Application.Reports.Business.TimingAndActuation;
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
//    public class TimingAndActuationController : ControllerBase
//    {
//        private readonly TimingAndActuationsForPhaseService timingAndActuationsForPhaseService;
//        private readonly IApproachRepository approachRepository;
//        private readonly IControllerEventLogRepository controllerEventLogRepository;

//        public TimingAndActuationController(
//            TimingAndActuationsForPhaseService timingAndActuationsForPhaseService,
//            IApproachRepository approachRepository,
//            IControllerEventLogRepository controllerEventLogRepository
//            )
//        {
//            this.timingAndActuationsForPhaseService = timingAndActuationsForPhaseService;
//            this.approachRepository = approachRepository;
//            this.controllerEventLogRepository = controllerEventLogRepository;
//        }

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public TimingAndActuationsForPhaseResult Test()
//        {
//            Fixture fixture = new();
//            TimingAndActuationsForPhaseResult viewModel = fixture.Create<TimingAndActuationsForPhaseResult>();
//            return viewModel;
//        }



//        [HttpPost("getChartData")]
//        public TimingAndActuationsForPhaseResult GetChartData([FromBody] TimingAndActuationsOptions options)
//        {
//            var approach = approachRepository.Lookup(options.ApproachId);
//            var eventCodes = new List<int> { };
//            if (options.ShowAdvancedCount || options.ShowAdvancedDilemmaZone || options.ShowLaneByLaneCount || options.ShowStopBarPresence)
//                eventCodes.AddRange(new List<int> { 81, 82 });
//            if (options.ShowPedestrianActuation)
//                eventCodes.AddRange(new List<int> { 89, 90 });
//            if (options.ShowPedestrianIntervals)
//                eventCodes.AddRange(timingAndActuationsForPhaseService.GetPedestrianIntervalEventCodes(approach.IsPedestrianPhaseOverlap));
//            if (options.PhaseEventCodesList != null)
//                eventCodes.AddRange(options.PhaseEventCodesList);
//            eventCodes.AddRange(timingAndActuationsForPhaseService.GetCycleCodes(
//                (options.GetPermissivePhase && approach.IsPermissivePhaseOverlap) ||
//                (!options.GetPermissivePhase && approach.IsPermissivePhaseOverlap))
//                );
//            var controllerEventLogs = controllerEventLogRepository.GetEventsByEventCodesParam(
//                approach.SignalId,
//                options.Start,
//                options.End,
//                eventCodes,
//                options.PhaseNumber).ToList();
//            var result = timingAndActuationsForPhaseService.GetChartData(options, approach, controllerEventLogs);
//            return result;
//        }

//    }
//}
