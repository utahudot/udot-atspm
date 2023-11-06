using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.SplitFail;
using ATSPM.ReportApi.TempExtensions;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.ReportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SplitFailController : ControllerBase
    {
        private readonly SplitFailPhaseService splitFailPhaseService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;
        private readonly PhaseService phaseService;

        public SplitFailController(
            SplitFailPhaseService splitFailPhaseService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository,
            PhaseService phaseService
            )
        {
            this.splitFailPhaseService = splitFailPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
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
        public async Task<IActionResult> GetChartData([FromBody] SplitFailOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalIdentifier, options.Start);
            if (signal == null)
            {
                return BadRequest("Signal not found");
            }
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, options.Start.AddHours(-12), options.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                return Ok("No Controller Event Logs found for signal");
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
               options.Start.AddHours(-12),
               options.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<IEnumerable<SplitFailsResult>>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(options, phase, controllerEventLogs, planEvents, false));
            }

            var results = await Task.WhenAll(tasks);
            var finalResultcheck = results.Where(result => result != null).SelectMany(r => r).ToList();

            if (finalResultcheck.IsNullOrEmpty())
            {
                return Ok("No chart data found");
            }
            return Ok(finalResultcheck);
        }

        private async Task<IEnumerable<SplitFailsResult>> GetChartDataForApproach(
            SplitFailOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            bool usePermissivePhase)
        {
            //var cycleEventCodes = approach.GetCycleEventCodes(options.UsePermissivePhase);
            var cycleEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(
                phaseDetail.PhaseNumber,
                options.UsePermissivePhase,
                options.Start,
                options.End);
            if (cycleEvents.IsNullOrEmpty())
                return null;
            var terminationEvents = controllerEventLogs.GetEventsByEventCodes(
                 options.Start,
                 options.End,
                 new List<int> { 4, 5, 6 },
                 phaseDetail.PhaseNumber);
            var detectors = phaseDetail.Approach.GetDetectorsForMetricType(options.MetricTypeId);
            var tasks = new List<Task<SplitFailsResult>>();
            foreach (var detectionType in detectors.SelectMany(d => d.DetectionTypes).Distinct())
            {
                tasks.Add(GetChartDataByDetectionType(options, phaseDetail, controllerEventLogs, planEvents, cycleEvents, terminationEvents, detectors, detectionType));
            }
            var results = await Task.WhenAll(tasks);
            return results.Where(result => result != null);
        }

        private async Task<SplitFailsResult> GetChartDataByDetectionType(
            SplitFailOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            IReadOnlyList<ControllerEventLog> cycleEvents,
            IReadOnlyList<ControllerEventLog> terminationEvents,
            List<Detector> detectors,
            DetectionType detectionType)
        {
            var tempDetectorEvents = controllerEventLogs.GetDetectorEvents(
               options.MetricTypeId,
               phaseDetail.Approach,
               options.Start,
               options.End,
               true,
               true,
               detectionType);
            if (tempDetectorEvents == null)
            {
                return null;
            }
            var detectorEvents = tempDetectorEvents.ToList();
            AddBeginEndEventsByDetector(options, detectors, detectionType, detectorEvents);
            var splitFailData = splitFailPhaseService.GetSplitFailPhaseData(
                options,
                cycleEvents,
                planEvents,
                terminationEvents,
                detectorEvents,
                phaseDetail.Approach);
            var result = new SplitFailsResult(
                options.SignalIdentifier,
                phaseDetail.Approach.Id,
                phaseDetail.PhaseNumber,
                options.Start,
                options.End,
                splitFailData.TotalFails,
                splitFailData.Plans,
                splitFailData.Cycles.Where(c => c.IsSplitFail).Select(c => new DataPointBase(c.StartTime)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.GapOut)
                    .Select(b => new DataPointForDouble(b.StartTime, b.GreenOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.GapOut)
                    .Select(b => new DataPointForDouble(b.StartTime, b.RedOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.ForceOff)
                    .Select(b => new DataPointForDouble(b.StartTime, b.GreenOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.ForceOff)
                    .Select(b => new DataPointForDouble(b.StartTime, b.RedOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new DataPointForDouble(b.StartTime, b.AverageGreenOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new DataPointForDouble(b.StartTime, b.AverageRedOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new DataPointForDouble(b.StartTime, b.PercentSplitfails)).ToList()
                );
            result.ApproachDescription = phaseDetail.Approach.Description;
            result.SignalDescription = phaseDetail.Approach.Signal.SignalDescription();
            return result;
        }

        private static void AddBeginEndEventsByDetector(SplitFailOptions options, List<Detector> detectors, DetectionType detectionType, List<ControllerEventLog> detectorEvents)
        {
            foreach (Detector channel in detectors.Where(d => d.DetectionTypes.Contains(detectionType)))
            {
                //add an EC 82 at the beginning if the first EC code is 81
                var firstEvent = detectorEvents.Where(d => d.EventParam == channel.DetectorChannel).FirstOrDefault();
                var lastEvent = detectorEvents.Where(d => d.EventParam == channel.DetectorChannel).LastOrDefault();

                if (firstEvent != null && firstEvent.EventCode == 81)
                {
                    var newDetectorOn = new ControllerEventLog();
                    newDetectorOn.SignalIdentifier = options.SignalIdentifier;
                    newDetectorOn.Timestamp = options.Start;
                    newDetectorOn.EventCode = 82;
                    newDetectorOn.EventParam = channel.DetectorChannel;
                    detectorEvents.Add(newDetectorOn);
                }

                //add an EC 81 at the end if the last EC code is 82
                if (lastEvent != null && lastEvent.EventCode == 82)
                {
                    var newDetectorOn = new ControllerEventLog();
                    newDetectorOn.SignalIdentifier = options.SignalIdentifier;
                    newDetectorOn.Timestamp = options.End;
                    newDetectorOn.EventCode = 81;
                    newDetectorOn.EventParam = channel.DetectorChannel;
                    detectorEvents.Add(newDetectorOn);
                }
            }
        }
    }
}
