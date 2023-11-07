using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.ApproachVolume;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Approach delay report service
    /// </summary>
    public class ApproachVolumeReportService : ReportServiceBase<ApproachVolumeOptions, IEnumerable<ApproachVolumeResult>>
    {
        private readonly ISignalRepository signalRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ApproachVolumeService approachVolumeService;

        /// <inheritdoc/>
        public ApproachVolumeReportService(
            ISignalRepository signalRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            ApproachVolumeService approachVolumeService)
        {
            this.signalRepository = signalRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.approachVolumeService = approachVolumeService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<ApproachVolumeResult>> ExecuteAsync(ApproachVolumeOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);

            if (signal == null)
            {
                //return BadRequest("Signal not found");
                return await Task.FromException<IEnumerable<ApproachVolumeResult>>(new NullReferenceException("Signal not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<ApproachVolumeResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var tasks = new List<Task<ApproachVolumeResult>>();
            var nbSbApproaches = signal.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.NB || a.DirectionTypeId == DirectionTypes.SB)).ToList();
            GetApproachVolume(parameter, signal, controllerEventLogs, tasks, nbSbApproaches);
            var ebWbApproaches = signal.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.EB || a.DirectionTypeId == DirectionTypes.WB)).ToList();
            GetApproachVolume(parameter, signal, controllerEventLogs, tasks, ebWbApproaches);
            var nwbSebApproaches = signal.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.NW || a.DirectionTypeId == DirectionTypes.SE)).ToList();
            GetApproachVolume(parameter, signal, controllerEventLogs, tasks, nwbSebApproaches);
            var nebSwbApproaches = signal.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.NE || a.DirectionTypeId == DirectionTypes.SW)).ToList();
            GetApproachVolume(parameter, signal, controllerEventLogs, tasks, nebSwbApproaches);
            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return results;
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
