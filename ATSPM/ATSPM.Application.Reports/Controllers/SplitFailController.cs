using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.SplitFail;
using ATSPM.Application.Repositories;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System;
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

        public SplitFailController(
            SplitFailPhaseService splitFailPhaseService,
            ISignalRepository signalRepository)
        {
            this.splitFailPhaseService = splitFailPhaseService;
            this.signalRepository = signalRepository;
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
            var splitFailData = splitFailPhaseService.GetSplitFailPhaseData(options);
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

    }
}
