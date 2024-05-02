using ATSPM.Application.Business;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.TurningMovementCounts;
using ATSPM.Application.Business.TurningMovementCounts.MOE.Common.Business.TMC;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Turning movement count report service
    /// </summary>
    public class TurningMovementCountReportService : ReportServiceBase<TurningMovementCountsOptions, TurningMovementCountsResult>
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly TurningMovementCountsService turningMovementCountsService;
        private readonly ILocationRepository LocationRepository;
        private readonly PlanService planService;

        /// <inheritdoc/>
        public TurningMovementCountReportService(
            IIndianaEventLogRepository controllerEventLogRepository,
            TurningMovementCountsService turningMovementCountsService,
            ILocationRepository LocationRepository,
            PlanService planService
            )
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.turningMovementCountsService = turningMovementCountsService;
            this.LocationRepository = LocationRepository;
            this.planService = planService;
        }

        /// <inheritdoc/>
        public override async Task<TurningMovementCountsResult> ExecuteAsync(TurningMovementCountsOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);

            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<TurningMovementCountsResult>(new NullReferenceException("Location not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<TurningMovementCountsResult>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var plans = planService.GetBasicPlans(parameter.Start, parameter.End, parameter.LocationIdentifier, planEvents);
            var tasks = new List<Task<IEnumerable<TurningMovementCountsLanesResult>>>();
            foreach (var laneType in Enum.GetValues(typeof(LaneTypes)))
            {
                tasks.Add(
                    GetChartDataForLaneType(
                    Location,
                (LaneTypes)laneType,
                    parameter,
                    controllerEventLogs,
                    plans.ToList())
                    );
            }
            var results = await Task.WhenAll(tasks);

            var finalLaneResultcheck = results.Where(result => result != null).SelectMany(r => r).ToList();

            var finalResultcheck = new TurningMovementCountsResult
            {
                Charts = finalLaneResultcheck,
                Table = new List<TurningMovementCountData>()
            };

            //Get Lane results by direction and movement type and bin size anc create a list of TurningMovementCountData for each direction and movement type
            foreach (var direction in Location.Approaches.Select(a => a.DirectionTypeId).Distinct())
            {
                var laneResultsByDirection = finalLaneResultcheck.Where(r => r.Direction == direction.GetAttributeOfType<DisplayAttribute>().Name).ToList();
                var movementTypes = laneResultsByDirection.Select(r => r.MovementType).Distinct().ToList();
                foreach (var movementType in movementTypes)
                {
                    var laneResultsByMovementType = laneResultsByDirection.Where(r => r.MovementType == movementType).ToList();
                    if (laneResultsByMovementType.IsNullOrEmpty())
                    {
                        continue;
                    }
                    var turningMovementCountData = new TurningMovementCountData { Direction = direction.GetAttributeOfType<DisplayAttribute>().Name, LaneType = laneResultsByMovementType.FirstOrDefault().LaneType, MovementType = movementType };

                    //sum the totalVolumes.value grouped by toalVolume.Start and add to turningMovementCountData.Volumes
                    turningMovementCountData.Volumes = laneResultsByMovementType
                        .SelectMany(r => r.TotalVolumes)
                        .GroupBy(v => v.Timestamp)
                        .Select(g => new DataPointForInt(g.Key, g.Sum(v => v.Value)))
                        .ToList();
                    finalResultcheck.Table.Add(turningMovementCountData);
                }
            }
            return finalResultcheck;
        }

        private async Task<IEnumerable<TurningMovementCountsLanesResult>> GetChartDataForLaneType(
            Location Location,
            LaneTypes laneType,
            TurningMovementCountsOptions options,
            List<IndianaEvent> controllerEventLogs,
            List<Plan> plans)
        {
            if (!Location.Approaches.SelectMany(a => a.Detectors).Select(d => d.LaneType).Distinct().Contains(laneType))
            {
                return null;
            }
            var directions = Location.Approaches.Select(a => a.DirectionTypeId).Distinct().ToList();
            var tasks = new List<Task<TurningMovementCountsLanesResult>>();
            foreach (var direction in directions)
            {
                var detectorsForDirection = Location.Approaches.Where(a => a.DirectionTypeId == direction).SelectMany(a => a.GetDetectorsForMetricType(options.MetricTypeId)).ToList();

                var movementTypesSorted = new List<MovementTypes> { MovementTypes.L, MovementTypes.T, MovementTypes.R };
                foreach (var movementType in movementTypesSorted)
                {
                    var movementTypeDetectors = new List<Detector>();
                    if (movementType == MovementTypes.T)
                    {
                        movementTypeDetectors = detectorsForDirection.Where(d =>
                        d.MovementType == MovementTypes.T
                        || d.MovementType == MovementTypes.TL
                        || d.MovementType == MovementTypes.TR).ToList();
                    }
                    else
                    {
                        movementTypeDetectors = detectorsForDirection.Where(d => d.MovementType == movementType).ToList();
                    }
                    if (!movementTypeDetectors.IsNullOrEmpty())
                    {
                        tasks.Add(GetChartDataByMovementType(
                            options,
                            plans,
                            controllerEventLogs,
                            movementTypeDetectors,
                            movementType,
                            laneType,
                            Location.LocationIdentifier,
                            Location.LocationDescription(),
                            direction));
                    }
                }
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null).OrderBy(r => r.Direction).ThenBy(r => r.MovementType);
        }

        private async Task<TurningMovementCountsLanesResult> GetChartDataByMovementType(
            TurningMovementCountsOptions options,
            List<Plan> planEvents,
            List<IndianaEvent> controllerEventLogs,
            List<Detector> detectors,
            MovementTypes movementType,
            LaneTypes laneType,
            string locationIdentifier,
            string LocationDescription,
            DirectionTypes directionType)
        {
            var detectorEvents = new List<IndianaEvent>();
            foreach (var detector in detectors)
            {
                detectorEvents.AddRange(controllerEventLogs.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
                    options.Start,
                    options.End,
                    new List<short>() { 82 },
                    detector.DetectorChannel,
                    detector.GetOffset(),
                    detector.LatencyCorrection).ToList());
            }
            var result = turningMovementCountsService.GetChartData(
                detectors,
                laneType,
                movementType,
                directionType,
                options,
                detectorEvents,
                planEvents,
                locationIdentifier,
                LocationDescription);

            return await result;
        }
    }
}
