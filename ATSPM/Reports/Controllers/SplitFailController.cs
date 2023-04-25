using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.SplitFail;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.Infrastructure.Repositories;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SplitFailController : ControllerBase
    {
        private readonly SplitFailPhaseService splitFailPhaseService;
        private readonly ISignalRepository signalRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly IApproachRepository approachRepository;

        public SplitFailController(
            SplitFailPhaseService splitFailPhaseService,
            IControllerEventLogRepository controllerEventLogRepository,
            IApproachRepository approachRepository)
        {
            this.splitFailPhaseService = splitFailPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.approachRepository = approachRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public SplitFailsResult Test()
        {
            Fixture fixture = new();
            SplitFailsResult viewModel = fixture.Create<SplitFailsResult>();
            return viewModel;
        }


        [HttpPost("getChartData")]
        public SplitFailsResult GetChartData([FromBody] SplitFailOptions options, List<ControllerEventLog> planEvents)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalId, options.StartDate);

            var approach = approachRepository.Lookup(options.ApproachId);
            var cycleEventCodes = approach.GetCycleEventCodes(options.UsePermissivePhase);
            var cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(
                approach.SignalId,
                options.StartDate,
                options.EndDate,
                cycleEventCodes,
                options.UsePermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber);
            var terminationEvents = controllerEventLogRepository.GetEventsByEventCodesParam(
                approach.SignalId,
                options.StartDate,
                options.EndDate,
                new List<int> { 4, 5, 6 },
                options.UsePermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber);
            var detectorEvents = controllerEventLogRepository.GetDetectorEvents(
                12,
                approach,
                options.StartDate,
                options.EndDate,
                true,
                true).ToList();
            //I think this is trying to add 81,82 to the list of events if it finds it before the start dateTime. The way this is done 
            //does not make sense since it adds an event at the start and end date time. I think it should be something like this:
            //Add a 82, 81 to the list of events at the start date time if it finds it before the start date time. Need to verify with
            //a traffic engineer.
            //if(EventsBeforeStart(options.StartDate, detector))
            //{
            //    detectorEvents.Add(new SplitFailDetectorActivation
            //    {
            //        DetectorOn = options.StartDate,
            //        DetectorOff = options.EndDate
            //    });
            //}
            var splitFailData = splitFailPhaseService.GetSplitFailPhaseData(
                options,
                cycleEvents,
                planEvents,
                terminationEvents,
                detectorEvents,
                approach);
            return new SplitFailsResult(
                "Split Fail Chart",
                options.SignalId,
                signal.SignalDescription(),
                options.UsePermissivePhase ? splitFailData.Approach.PermissivePhaseNumber.Value : splitFailData.Approach.ProtectedPhaseNumber,
                splitFailData.Approach.Description,
                options.StartDate,
                options.EndDate,
                splitFailData.TotalFails,
                splitFailData.Plans,
                splitFailData.Bins.Select(b => new FailLine(b.StartTime, Convert.ToInt32(b.SplitFails))).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.GapOut)
                    .Select(b => new GapOutGreenOccupancy(b.StartTime, b.GreenOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.GapOut)
                    .Select(b => new GapOutRedOccupancy(b.StartTime, b.RedOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.ForceOff)
                    .Select(b => new ForceOffGreenOccupancy(b.StartTime, b.GreenOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.ForceOff)
                    .Select(b => new ForceOffRedOccupancy(b.StartTime, b.RedOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new AverageGor(b.StartTime, b.AverageGreenOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new AverageRor(b.StartTime, b.AverageRedOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new PercentFail(b.StartTime, b.PercentSplitfails)).ToList()
                );
        }      



        private bool EventsBeforeStart(
            SplitFailOptions options,
            Detector detector,
            SplitFailPhaseData splitFailPhaseData)
        {
            var eventOnBeforeStart = controllerEventLogRepository.GetFirstEventBeforeDateByEventCodeAndParameter(
                options.SignalId,
                detector.DetChannel, 81, options.StartDate);
            var eventOffBeforeStart = controllerEventLogRepository.GetFirstEventBeforeDateByEventCodeAndParameter(
                options.SignalId,
                detector.DetChannel, 82, options.StartDate);
            return (eventOnBeforeStart != null && eventOffBeforeStart == null); 
                
        }

    }
}
