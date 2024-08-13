﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.ReportServices/TurningMovementCountReportService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
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
                    var turningMovementCountData = new TurningMovementCountData
                    {
                        Direction = direction.GetAttributeOfType<DisplayAttribute>().Name,
                        LaneType = laneResultsByMovementType.FirstOrDefault().LaneType,
                        MovementType = movementType
                    };

                    //sum the totalVolumes.value grouped by toalVolume.Start and add to turningMovementCountData.Volumes
                    turningMovementCountData.Volumes = laneResultsByMovementType
                        .SelectMany(r => r.TotalVolumes)
                        .GroupBy(v => v.Timestamp)
                        .Select(g => new DataPointForInt(g.Key, g.Sum(v => v.Value)))
                        .ToList();
                    finalResultcheck.Table.Add(turningMovementCountData);
                }
            }
            finalResultcheck.PeakHour = FindPeakHour(finalResultcheck.Table);
            SetPeakHourFactor(finalResultcheck);
            SetPeakHourVolume(finalResultcheck);
            return finalResultcheck;
        }

        private void SetPeakHourVolume(TurningMovementCountsResult turningMovementCountsResult)
        {
            foreach (var lane in turningMovementCountsResult.Table)
            {
                lane.PeakHourVolume = new DataPointForInt(turningMovementCountsResult.PeakHour.Key, lane.Volumes
                    .Where(t => t.Timestamp >= turningMovementCountsResult.PeakHour.Key
                                                   && t.Timestamp < turningMovementCountsResult.PeakHour.Key.AddHours(1))
                    .Sum(t => t.Value));
            }
        }

        private void SetPeakHourFactor(TurningMovementCountsResult turningMovementCountsResult)
        {
            try
            {
                if (turningMovementCountsResult.Table
                        .Where(t => t.LaneType == "Vehicle")
                        .SelectMany(t => t.Volumes)
                        .Where(t => t.Timestamp >= turningMovementCountsResult.PeakHour.Key
                                    && t.Timestamp < turningMovementCountsResult.PeakHour.Key.AddHours(1))
                        .Count() > 0)
                {
                    var maxCount = turningMovementCountsResult.Table
                        .Where(t => t.LaneType == "Vehicle")
                        .SelectMany(t => t.Volumes)
                        .GroupBy(t => t.Timestamp)
                        .Select(t => new { Id = t.Key, Count = t.Sum(y => y.Value) })
                        .Max(t => t.Count);
                    double denominator = 4 * maxCount;
                    if (denominator != 0)
                        turningMovementCountsResult.PeakHourFactor = Math.Round(turningMovementCountsResult.PeakHour.Value / denominator, 2);
                }
            }
            catch
            {
                throw new Exception("Error Setting Peak Hour");
            }
        }

        private KeyValuePair<DateTime, int> FindPeakHour(List<TurningMovementCountData> turnningMovementCountData)
        {
            var binStartTimes = turnningMovementCountData.SelectMany(t => t.Volumes).Select(v => v.Timestamp).Distinct().OrderBy(r => r).ToList();
            var totalVolume = new KeyValuePair<DateTime, int>(DateTime.MinValue, 0);

            foreach (var date in binStartTimes)
            {
                var tempVolume = turnningMovementCountData
                    .Where(t => t.LaneType == "Vehicle")
                    .SelectMany(t => t.Volumes)
                    .Where(t =>
                        t.Timestamp >= date
                        && t.Timestamp < date.AddHours(1))
                    .Sum(t => t.Value);
                if (tempVolume > totalVolume.Value)
                    totalVolume = new KeyValuePair<DateTime, int>(date, tempVolume);
            }
            return totalVolume;
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
