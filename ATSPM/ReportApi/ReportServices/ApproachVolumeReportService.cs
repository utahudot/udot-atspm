using ATSPM.Application.Business;
using ATSPM.Application.Business.ApproachVolume;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Approach delay report service
    /// </summary>
    public class ApproachVolumeReportService : ReportServiceBase<ApproachVolumeOptions, IEnumerable<ApproachVolumeResult>>
    {
        private readonly ILocationRepository LocationRepository;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ApproachVolumeService approachVolumeService;

        /// <inheritdoc/>
        public ApproachVolumeReportService(
            ILocationRepository LocationRepository,
            IIndianaEventLogRepository controllerEventLogRepository,
            ApproachVolumeService approachVolumeService)
        {
            this.LocationRepository = LocationRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.approachVolumeService = approachVolumeService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<ApproachVolumeResult>> ExecuteAsync(ApproachVolumeOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);

            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<ApproachVolumeResult>>(new NullReferenceException("Location not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<ApproachVolumeResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var tasks = new List<Task<ApproachVolumeResult>>();
            var nbSbApproaches = Location.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.NB || a.DirectionTypeId == DirectionTypes.SB)).ToList();
            GetApproachVolume(parameter, Location, controllerEventLogs, tasks, nbSbApproaches, DirectionTypes.NB, DirectionTypes.SB);
            var ebWbApproaches = Location.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.EB || a.DirectionTypeId == DirectionTypes.WB)).ToList();
            GetApproachVolume(parameter, Location, controllerEventLogs, tasks, ebWbApproaches, DirectionTypes.EB, DirectionTypes.WB);
            var nwbSebApproaches = Location.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.NW || a.DirectionTypeId == DirectionTypes.SE)).ToList();
            GetApproachVolume(parameter, Location, controllerEventLogs, tasks, nwbSebApproaches, DirectionTypes.NW, DirectionTypes.SE);
            var nebSwbApproaches = Location.Approaches.Where(a => a.ProtectedPhaseNumber != 0 && (a.DirectionTypeId == DirectionTypes.NE || a.DirectionTypeId == DirectionTypes.SW)).ToList();
            GetApproachVolume(parameter, Location, controllerEventLogs, tasks, nebSwbApproaches, DirectionTypes.NE, DirectionTypes.SW);
            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).OrderBy(r => r.PrimaryDirectionName).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return finalResultcheck;
        }

        private void GetApproachVolume(
            ApproachVolumeOptions options,
            Location Location,
            List<IndianaEvent> controllerEventLogs,
            List<Task<ApproachVolumeResult>> tasks,
            List<Approach> approaches,
            DirectionTypes primaryDirection,
            DirectionTypes opposingDirection
            )
        {
            if (approaches.Any())
            {
                var detectionTypes = approaches.SelectMany(a => a.GetDetectorsForMetricType(options.MetricTypeId)).SelectMany(d => d.DetectionTypes).Distinct().ToList();
                foreach (var detectionType in detectionTypes.Where(d => d.Id == DetectionTypes.AC || d.Id == DetectionTypes.LLC))
                {
                    GetAppoachVolumeForApproaches(options, Location, controllerEventLogs, tasks, approaches, detectionType, primaryDirection, opposingDirection);
                }
            }
        }

        private void GetAppoachVolumeForApproaches(
            ApproachVolumeOptions options,
            Location Location,
            List<IndianaEvent> controllerEventLogs,
            List<Task<ApproachVolumeResult>> tasks,
            List<Approach> approaches,
            DetectionType detectionType,
            DirectionTypes primaryDirection,
            DirectionTypes opposingDirection
            )
        {
            var primaryApproaches = approaches.Where(a => a.DirectionTypeId == primaryDirection && a.Detectors.SelectMany(d => d.DetectionTypes).Contains(detectionType)).ToList();
            var opposingApproaches = approaches.Where(a => a.DirectionTypeId == opposingDirection && a.Detectors.SelectMany(d => d.DetectionTypes).Contains(detectionType)).ToList();
            tasks.Add(GetApproachVolumeByDetectionType(
                options,
                Location,
                controllerEventLogs,
                primaryApproaches,
                opposingApproaches,
                detectionType));
        }

        private async Task<ApproachVolumeResult> GetApproachVolumeByDetectionType(
            ApproachVolumeOptions options,
            Location Location,
            List<IndianaEvent> controllerEventLogs,
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
            List<IndianaEvent> primaryDetectorEvents = GetDetectorEvents(options, out primaryDistanceFromStopBar, controllerEventLogs, primaryApproaches, detectionType);
            List<IndianaEvent> opposingDetectorEvents = GetDetectorEvents(options, out opposingDistanceFromStopBar, controllerEventLogs, opposingApproaches, detectionType);
            if (primaryDetectorEvents.Count == 0 && opposingDetectorEvents.Count == 0)
            {
                return new ApproachVolumeResult(Location.LocationIdentifier, options.Start, options.End, primaryApproaches.FirstOrDefault().DirectionTypeId);
            }
            ApproachVolumeResult viewModel = approachVolumeService.GetChartData(
                options,
                Location,
                primaryDetectorEvents,
                opposingDetectorEvents,
                primaryApproaches,
                opposingApproaches,
                primaryDistanceFromStopBar,
                detectionType);
            viewModel.LocationDescription = Location.LocationDescription();
            return viewModel;
        }

        private List<IndianaEvent> GetDetectorEvents(
            ApproachVolumeOptions options,
            out int distanceFromStopBar,
            List<IndianaEvent> controllerEventLogs,
            List<Approach> approaches,
            DetectionType detectionType)
        {

            var detectors = approaches
                .SelectMany(a => a.Detectors)
                .Where(d => d.DetectionTypes.Any(dt => dt.Id == detectionType.Id) && (d.LaneType == LaneTypes.V || d.LaneType == LaneTypes.NA))
                .ToList();
            distanceFromStopBar = 0;
            if (detectors.Any())
            {
                distanceFromStopBar = detectors.First().DistanceFromStopBar ?? 0;
            }
            var detectorEvents = new List<IndianaEvent>();
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
