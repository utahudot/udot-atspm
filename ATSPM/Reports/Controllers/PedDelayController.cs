//using ATSPM.Application.Reports.Business.Common;
//using ATSPM.Application.Reports.Business.PedDelay;
//using ATSPM.Application.Repositories;
//using ATSPM.Application.Extensions;
//using AutoFixture;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Linq;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ATSPM.Application.Reports.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PedDelayController : ControllerBase
//    {
//        private readonly PedDelayService pedDelayService;
//        private readonly PedPhaseService pedPhaseService;
//        private readonly CycleService cycleService;
//        private readonly IApproachRepository approachRepository;
//        private readonly IControllerEventLogRepository controllerEventLogRepository;

//        public PedDelayController(
//            PedDelayService pedDelayService,
//            PedPhaseService pedPhaseService,
//            CycleService cycleService,
//            IApproachRepository approachRepository,
//            IControllerEventLogRepository controllerEventLogRepository)
//        {
//            this.pedDelayService = pedDelayService;
//            this.pedPhaseService = pedPhaseService;
//            this.cycleService = cycleService;
//            this.approachRepository = approachRepository;
//            this.controllerEventLogRepository = controllerEventLogRepository;
//        }

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public PedDelayResult Test()
//        {
//            Fixture fixture = new();
//            PedDelayResult viewModel = fixture.Create<PedDelayResult>();
//            return viewModel;
//        }

//        [HttpGet("GetChartData")]
//        public PedDelayResult GetChartData(
//            PedDelayOptions options)
//        {
//            var approach = approachRepository.Lookup(options.ApproachId);
//            var planEvents = controllerEventLogRepository.GetPlanEvents(
//                approach.SignalId,
//                options.Start,
//                options.End);

//            var pedEvents = controllerEventLogRepository.GetRecordsByParameterAndEvent(
//                approach.SignalId,
//                options.Start.AddMinutes(-15),
//                options.End,
//                approach.GetPedDetectorsFromApproach(),
//                approach.GetPedestrianCycleEventCodes());

//            var cycleEvents = controllerEventLogRepository.GetCycleEventsWithTimeExtension(
//                approach,
//                options.UsePermissivePhase,
//                options.Start,
//                options.End);

//            var pedPhaseData = pedPhaseService.GetPedPhaseData(
//                options,
//                approach,
//                planEvents.ToList(),
//                pedEvents.ToList());

//            var cycles = cycleService.GetRedToRedCycles(
//                options.Start,
//                options.End,
//                cycleEvents.ToList());

//            PedDelayResult viewModel = pedDelayService.GetChartData(
//                options,
//                pedPhaseData,
//                cycles
//                );
//            return viewModel;
//        }

//    }
//}
