#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/TurningMovementCountReportService.cs
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
            ComputePeakHourAndFactor(finalResultcheck, parameter.Start, parameter.End, parameter.BinSize);
            SetPeakHourVolume(finalResultcheck);
            return finalResultcheck;
        }

        private void SetPeakHourVolume(TurningMovementCountsResult result)
        {
            if (!result.PeakHour.HasValue)
            {
                foreach (var lane in result.Table)
                    lane.PeakHourVolume = null;
                return;
            }

            var peakStart = result.PeakHour.Value.Key;
            const int AGG = 15;
            const int QUARTS = 4;

            foreach (var lane in result.Table)
            {
                int hourTotal = 0;

                for (int i = 0; i < QUARTS; i++)
                {
                    var binStart = peakStart.AddMinutes(i * AGG);
                    var binEnd = binStart.AddMinutes(AGG);

                    hourTotal += lane.Volumes
                        .Where(v => v.Timestamp >= binStart && v.Timestamp < binEnd)
                        .Sum(v => v.Value);
                }

                lane.PeakHourVolume = new DataPointForInt(peakStart, hourTotal);
            }
        }


        private void ComputePeakHourAndFactor(
             TurningMovementCountsResult result,
             DateTime periodStart,
             DateTime periodEnd,
             int binSizeMinutes)
        {
            if (60 % binSizeMinutes != 0 || (periodEnd - periodStart).TotalMinutes < 60)
            {
                result.PeakHour = null;
                result.PeakHourFactor = null;
                return;
            }

            var allBins = result.Table
                .Where(l => l.LaneType == "Vehicle")
                .SelectMany(l => l.Volumes)
                .Where(v => v.Timestamp >= periodStart && v.Timestamp < periodEnd)
                .GroupBy(v => v.Timestamp)
                .Select(g => new { Time = g.Key, Sum = g.Sum(v => v.Value) })
                .OrderBy(x => x.Time)
                .ToList();

            int binsPerHour = 60 / binSizeMinutes;
            if (allBins.Count < binsPerHour)
            {
                result.PeakHour = null;
                result.PeakHourFactor = null;
                return;
            }

            int bestSum = 0;
            DateTime bestStart = DateTime.MinValue;
            for (int i = 0; i + binsPerHour <= allBins.Count; i++)
            {
                int windowSum = 0;
                for (int j = 0; j < binsPerHour; j++)
                    windowSum += allBins[i + j].Sum;

                if (windowSum > bestSum)
                {
                    bestSum = windowSum;
                    bestStart = allBins[i].Time;
                }
            }

            result.PeakHour = new KeyValuePair<DateTime, int>(bestStart, bestSum);

            if (15 % binSizeMinutes != 0)
            {
                result.PeakHourFactor = null;
                return;
            }

            var hourBins = allBins
                .Where(x => x.Time >= bestStart && x.Time < bestStart.AddHours(1))
                .Select(x => x.Sum)
                .ToList();

            if (hourBins.Count == 0)
            {
                result.PeakHourFactor = null;
                return;
            }

            var quarterSums = new int[4];
            foreach (var x in allBins.Where(b => b.Time >= bestStart && b.Time < bestStart.AddHours(1)))
            {
                int minsPast = (int)(x.Time - bestStart).TotalMinutes;
                int idx = Math.Min(3, minsPast / 15);
                quarterSums[idx] += x.Sum;
            }

            int peakQuarter = quarterSums.Max();
            int denom = peakQuarter * 4;

            result.PeakHourFactor = denom == 0
                ? (double?)null
                : Math.Round((double)bestSum / denom, 2);
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

                var movementTypesSorted = new List<MovementTypes> { MovementTypes.L, MovementTypes.TL, MovementTypes.T, MovementTypes.TR, MovementTypes.R };
                foreach (var movementType in movementTypesSorted)
                {
                    var movementTypeDetectors = new List<Detector>();

                    movementTypeDetectors = detectorsForDirection.Where(d => d.MovementType == movementType).ToList();

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
