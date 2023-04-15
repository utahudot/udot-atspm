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
        public SplitFailsResult GetChartData([FromBody] SplitFailOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalId, options.StartDate);

            var approach = approachRepository.Lookup(options.ApproachId);
            var cycleEvents = GetCycleEvents(options.UsePermissivePhase, options.StartDate, options.EndDate, approach)
            var splitFailData = splitFailPhaseService.GetSplitFailPhaseData(options,cycleEvents,approach);
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


        /// <summary>
        /// Needs event codes 1,8,9,61,63,64,66
        /// </summary>
        /// <param name="getPermissivePhase"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="approach"></param>
        /// <returns></returns>
        private List<ControllerEventLog> GetCycleEvents(bool getPermissivePhase, DateTime startDate,
            DateTime endDate, Approach approach)
        {
            List<ControllerEventLog> cycleEvents;
            if (getPermissivePhase)
            {
                var cycleEventNumbers = approach.IsPermissivePhaseOverlap
                    ? new List<int> { 61, 63, 64, 66 }
                    : new List<int> { 1, 8, 9 };
                cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(approach.SignalId, startDate,
                    endDate, cycleEventNumbers, approach.PermissivePhaseNumber.Value).ToList();
            }
            else
            {
                var cycleEventNumbers = approach.IsProtectedPhaseOverlap
                    ? new List<int> { 61, 63, 64, 66 }
                    : new List<int> { 1, 8, 9 };
                cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(approach.SignalId, startDate,
                    endDate, cycleEventNumbers, approach.ProtectedPhaseNumber).ToList();
            }

            return cycleEvents;
        }

    }
}
