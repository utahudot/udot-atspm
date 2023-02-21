//using ATSPM.Application.Extensions;
//using ATSPM.Application.Reports.Business.PhaseTermination;
//using AutoFixture;
//using Microsoft.AspNetCore.Mvc;
//using System.Linq;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ATSPM.Application.Reports.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PhaseTerminationController : ControllerBase
//    {
//        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;

//        //public PhaseTerminationController(AnalysisPhaseCollectionService analysisPhaseCollectionService)
//        //{
//        //    this.analysisPhaseCollectionService = analysisPhaseCollectionService;
//        //}

//        // GET: api/<ApproachVolumeController>
//        [HttpGet("test")]
//        public PhaseTerminationResult Test()
//        {
//            Fixture fixture = new();
//            PhaseTerminationResult viewModel = fixture.Create<PhaseTerminationResult>();
//            return viewModel;
//        }



//        //[HttpPost("getChartData")]
//        //public PhaseTerminationResult GetChartData([FromBody] PhaseTerminationOptions options)
//        //{
//        //    var phaseCollectionData = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
//        //        options.SignalId,
//        //        options.StartDate,
//        //        options.EndDate);
//        //    var gapOuts = phaseCollectionData.AnalysisPhases.SelectMany(p => p.ConsecutiveGapOuts).Select(p => new GapOut(p.Timestamp, p.EventParam)).ToList();
//        //    var maxOuts = phaseCollectionData.AnalysisPhases.SelectMany(p => p.ConsecutiveMaxOut).Select(p => new MaxOut(p.Timestamp, p.EventParam)).ToList();
//        //    var forceOffs = phaseCollectionData.AnalysisPhases.SelectMany(p => p.ConsecutiveForceOff).Select(p => new ForceOff(p.Timestamp, p.EventParam)).ToList();
//        //    var pedWalkBegin = phaseCollectionData.AnalysisPhases.SelectMany(p => p.PedestrianEvents).Select(p => new PedWalkBegin(p.Timestamp, p.EventParam)).ToList();
//        //    var unknownEvents = phaseCollectionData.AnalysisPhases.SelectMany(p => p.UnknownTermination).Select(p => new UnknownTermination(p.Timestamp, p.EventParam)).ToList();
//        //    var plans = phaseCollectionData.Plans.Select(p => new Business.Common.Plan(p.PlanNumber.ToString(), p.StartTime, p.EndTime)).ToList();
//        //    return new PhaseTerminationResult(
//        //        "Phase Termination Chart",
//        //        phaseCollectionData.SignalId,
//        //        phaseCollectionData.Signal.SignalDescription(),
//        //        options.StartDate,
//        //        options.EndDate,
//        //        options.SelectedConsecutiveCount,
//        //        plans,
//        //        gapOuts,
//        //        maxOuts,
//        //        forceOffs,
//        //        pedWalkBegin,
//        //        unknownEvents
//        //        );
//        //}
//    }
//}
