using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PhaseTermination;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhaseTerminationController : ControllerBase
    {
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public PhaseTerminationController(
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository)
        {
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PhaseTerminationResult Test()
        {
            Fixture fixture = new();
            PhaseTerminationResult viewModel = fixture.Create<PhaseTerminationResult>();
            return viewModel;
        }



        [HttpPost("getChartData")]
        public PhaseTerminationResult GetChartData([FromBody] PhaseTerminationOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalId, options.Start);
            var signalEvents = controllerEventLogRepository.GetSignalEventsBetweenDates(options.SignalId, options.Start.AddHours(-12), options.End.AddHours(12));
            var planEvents = signalEvents.Where(e => e.EventCode == 131).ToList();
            var terminationEvents = signalEvents.Where(e =>
                new List<int> { 4, 5, 6, 7 }.Contains(e.EventCode)
                && e.Timestamp >= options.Start 
                && e.Timestamp <= options.End).ToList();
            var pedEvents = signalEvents.Where(e =>
                new List<int> { 21, 23 }.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            var cycleEvents = signalEvents.Where(e =>
                new List<int> { 1, 11 }.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            var splitsEventCodes = new List<int>();
            for (var i = 130; i <= 151; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = signalEvents.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            signalEvents = null;
            var phaseCollectionData = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                options.SignalId,
                options.Start,
                options.End,
                planEvents,
                cycleEvents,
                splitsEvents,
                terminationEvents,
                pedEvents,
                signal,
                options.SelectedConsecutiveCount);

            var gapOuts = phaseCollectionData.AnalysisPhases.SelectMany(p => p.ConsecutiveGapOuts)?.Select(p => new GapOut(p.Timestamp, p.EventParam)).ToList();
            var maxOuts = phaseCollectionData.AnalysisPhases.SelectMany(p => p.ConsecutiveMaxOut)?.Select(p => new MaxOut(p.Timestamp, p.EventParam)).ToList();
            var forceOffs = phaseCollectionData.AnalysisPhases.SelectMany(p => p.ConsecutiveForceOff)?.Select(p => new ForceOff(p.Timestamp, p.EventParam)).ToList();
            var pedWalkBegin = phaseCollectionData.AnalysisPhases.SelectMany(p => p.PedestrianEvents)?.Select(p => new PedWalkBegin(p.Timestamp, p.EventParam)).ToList();
            var unknownEvents = phaseCollectionData.AnalysisPhases.SelectMany(p => p.UnknownTermination)?.Select(p => new UnknownTermination(p.Timestamp, p.EventParam)).ToList();
            var plans = phaseCollectionData.Plans.Select(p => new Business.Common.Plan(p.PlanNumber.ToString(), p.StartTime, p.EndTime)).ToList();
            return new PhaseTerminationResult(
                phaseCollectionData.SignalId,
                options.Start,
                options.End,
                options.SelectedConsecutiveCount,
                plans,
                gapOuts,
                maxOuts,
                forceOffs,
                pedWalkBegin,
                unknownEvents
                );
        }
    }
}
