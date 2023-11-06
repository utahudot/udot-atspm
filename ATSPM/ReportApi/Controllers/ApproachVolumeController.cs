using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.ApproachVolume;
using ATSPM.ReportApi.TempExtensions;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

//For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.ReportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachVolumeController : ControllerBase
    {
        private readonly ISignalRepository signalRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ApproachVolumeService approachVolumeService;

        public ApproachVolumeController(
            ISignalRepository signalRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            ApproachVolumeService approachVolumeService)
        {
            this.signalRepository = signalRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.approachVolumeService = approachVolumeService;
        }

        //GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public ApproachVolumeResult Test()
        {
            Fixture fixture = new();
            ApproachVolumeResult approachVolumeViewModel = fixture.Create<ApproachVolumeResult>();
            return approachVolumeViewModel;
        }


        [HttpPost("getChartData")]
        public async Task<IActionResult> GetChartData([FromBody] ApproachVolumeOptions options)
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
            var tasks = new List<Task<ApproachVolumeResult>>();
            var nbSbApproaches = signal.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.NB || a.DirectionTypeId == DirectionTypes.SB)).ToList();
            GetApproachVolume(options, signal, controllerEventLogs, tasks, nbSbApproaches);
            var ebWbApproaches = signal.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.EB || a.DirectionTypeId == DirectionTypes.WB)).ToList();
            GetApproachVolume(options, signal, controllerEventLogs, tasks, ebWbApproaches);
            var nwbSebApproaches = signal.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.NW || a.DirectionTypeId == DirectionTypes.SE)).ToList();
            GetApproachVolume(options, signal, controllerEventLogs, tasks, nwbSebApproaches);
            var nebSwbApproaches = signal.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.NE || a.DirectionTypeId == DirectionTypes.SW)).ToList();
            GetApproachVolume(options, signal, controllerEventLogs, tasks, nebSwbApproaches);
            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).ToList();

            if (finalResultcheck.IsNullOrEmpty())
            {
                return Ok("No chart data found");
            }
            return Ok(finalResultcheck);
        }

        private void GetApproachVolume(ApproachVolumeOptions options, Signal signal, List<ControllerEventLog> controllerEventLogs, List<Task<ApproachVolumeResult>> tasks, List<Approach> approaches)
        {
            if (approaches.Any())
            {
                var detectionTypes = approaches.SelectMany(a => a.GetDetectorsForMetricType(options.MetricTypeId)).SelectMany(d => d.DetectionTypes).Distinct().ToList();
                foreach (var detectionType in detectionTypes)
                {
                    GetAppoachVolumeForApproaches(options, signal, controllerEventLogs, tasks, approaches, detectionType);
                }
            }
        }

        private void GetAppoachVolumeForApproaches(ApproachVolumeOptions options, Signal signal, List<ControllerEventLog> controllerEventLogs, List<Task<ApproachVolumeResult>> tasks, List<Approach> nbSbApproaches, DetectionType detectionType)
        {
            var primaryApproaches = nbSbApproaches.Where(a => a.DirectionTypeId == DirectionTypes.NB && a.Detectors.SelectMany(d => d.DetectionTypes).Contains(detectionType)).ToList();
            var opposingApproaches = nbSbApproaches.Where(a => a.DirectionTypeId == DirectionTypes.SB && a.Detectors.SelectMany(d => d.DetectionTypes).Contains(detectionType)).ToList();
            tasks.Add(GetApproachVolumeByDetectionType(
                options,
                signal,
                controllerEventLogs,
                primaryApproaches,
                opposingApproaches,
                detectionType));
        }

        private async Task<ApproachVolumeResult> GetApproachVolumeByDetectionType(
            ApproachVolumeOptions options,
            Signal signal,
            List<ControllerEventLog> controllerEventLogs,
            List<Approach> primaryApproaches,
            List<Approach> opposingApproaches,
            DetectionType detectionType)
        {
            if (!primaryApproaches.Any() || !opposingApproaches.Any())
            {
                return null;
            }
            int primaryDistanceFromStopBar = 0;
            int opposingDistanceFromStopBar = 0;
            List<ControllerEventLog> primaryDetectorEvents = GetDetectorEvents(options, out primaryDistanceFromStopBar, controllerEventLogs, primaryApproaches, detectionType);
            List<ControllerEventLog> opposingDetectorEvents = GetDetectorEvents(options, out opposingDistanceFromStopBar, controllerEventLogs, opposingApproaches, detectionType);
            if (primaryDetectorEvents.Count == 0 && opposingDetectorEvents.Count == 0)
            {
                return new ApproachVolumeResult(signal.SignalIdentifier, options.Start, options.End, primaryApproaches.FirstOrDefault().DirectionTypeId);
            }
            ApproachVolumeResult viewModel = approachVolumeService.GetChartData(
                options,
                signal,
                primaryDetectorEvents,
                opposingDetectorEvents,
                primaryDistanceFromStopBar);
            viewModel.SignalDescription = signal.SignalDescription();
            return viewModel;
        }

        private List<ControllerEventLog> GetDetectorEvents(
            ApproachVolumeOptions options,
            out int distanceFromStopBar,
            List<ControllerEventLog> controllerEventLogs,
            List<Approach> approaches,
            DetectionType detectionType)
        {

            var detectors = approaches
                .SelectMany(a => a.Detectors)
                .Where(d => d.DetectionTypes.Any(dt => dt.Id == options.DetectionType) && (d.LaneType == LaneTypes.V || d.LaneType == LaneTypes.NA))
                .ToList();
            distanceFromStopBar = 0;
            if (detectors.Any())
            {
                distanceFromStopBar = detectors.First().DistanceFromStopBar ?? 0;
            }
            var detectorEvents = new List<ControllerEventLog>();
            foreach (var approach in approaches)
            {
                detectorEvents.AddRange(controllerEventLogs.GetDetectorEvents(
                    options.MetricTypeId,
                    approach,
                    options.Start,
                    options.End,
                    true,
                    false,
                    detectionType));
            }
            return detectorEvents;
        }
    }
}
