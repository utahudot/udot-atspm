//using ATSPM.Application.Reports.Business.PedDelay;
//using ATSPM.Application.Repositories;
//using AutoFixture;
//using Legacy.Common.Business;
//using Legacy.Common.Business.PEDDelay;
//using Microsoft.AspNetCore.Mvc;
//using System;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ATSPM.Application.Reports.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PedDelayController : ControllerBase
//    {
//        private readonly PedDelayService pedDelayService;
//        private readonly PedPhaseService pedPhaseService;
//        private readonly PlansBaseService plansBaseService;
//        private readonly CycleService cycleService;
//        private readonly IApproachRepository approachRepository;

//        //public PedDelayController(
//        //    PedDelayService pedDelayService,
//        //    PedPhaseService pedPhaseService,
//        //    PlansBaseService plansBaseService,
//        //    CycleService cycleService,
//        //    IApproachRepository approachRepository)
//        //{
//        //    this.pedDelayService = pedDelayService;
//        //    this.pedPhaseService = pedPhaseService;
//        //    this.plansBaseService = plansBaseService;
//        //    this.cycleService = cycleService;
//        //    this.approachRepository = approachRepository;
//        //}

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public PedDelayResult Test()
//        {
//            Fixture fixture = new();
//            PedDelayResult viewModel = fixture.Create<PedDelayResult>();
//            return viewModel;
//        }

//        //[HttpGet("GetChartData")]
//        //public PedDelayResult GetChartData(
//        //    int approachId,
//        //    DateTime startDate,
//        //    DateTime endDate,
//        //    int timeBuffer,
//        //    bool showPedBeginWalk,
//        //    bool showCycleLength,
//        //    bool showPercentDelay,
//        //    bool showPedRecall,
//        //    int pedRecallThreshold)
//        //{
//        //    var approach = approachRepository.Lookup(approachId);
//        //    var options = new PedDelayOptions(
//        //        approachId,
//        //        startDate,
//        //        endDate,
//        //        timeBuffer,
//        //        showPedBeginWalk,
//        //        showCycleLength,
//        //        showPercentDelay,
//        //        showPedRecall,
//        //        pedRecallThreshold);

//        //    var plansData = plansBaseService.GetEvents(
//        //        approach.SignalId,
//        //        startDate,
//        //        endDate);

//        //    var pedPhaseData = pedPhaseService.GetPedPhaseData(
//        //        approach,
//        //        timeBuffer,
//        //        startDate,
//        //        endDate,
//        //        plansData);

//        //    var cycles = cycleService.GetRedToRedCycles(
//        //        approach,
//        //        startDate,
//        //        endDate);

//        //    PedDelayResult viewModel = pedDelayService.GetChartData(
//        //        options,
//        //        pedPhaseData,
//        //        cycles
//        //        );
//        //    return viewModel;
//        //}

//    }
//}
