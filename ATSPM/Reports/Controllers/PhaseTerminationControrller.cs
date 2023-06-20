using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PhaseTermination;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            GC.Collect();
            
            var phaseCollectionData = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                options.SignalId,
                options.Start,
                options.End,
                planEvents,
                cycleEvents,
                splitsEvents,
                pedEvents,
                signal,
                options.SelectedConsecutiveCount);
            var phases = new List<Phase>();
            foreach (var phase in phaseCollectionData.AnalysisPhases)
            {
                phases.Add(new Phase(
                    phase.PhaseNumber,
                    phase.ConsecutiveGapOuts.Select(g => g.Timestamp).ToList(),
                    phase.ConsecutiveMaxOut.Select(g => g.Timestamp).ToList(),
                    phase.ConsecutiveForceOff.Select(g => g.Timestamp).ToList(),
                    phase.PedestrianEvents.Select(g => g.Timestamp).ToList(),
                    phase.UnknownTermination.Select(g => g.Timestamp).ToList()
                    ));
            }

           var plans = phaseCollectionData.Plans.Select(p => new Plan(p.PlanNumber.ToString(), p.StartTime, p.EndTime)).ToList();
            return new PhaseTerminationResult(
                phaseCollectionData.SignalId,
                options.Start,
                options.End,
                options.SelectedConsecutiveCount,
                plans,
                phases
                );
        }
    }
}
