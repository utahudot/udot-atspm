#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeOfDay/TimeOfDayLocationService.cs
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

using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public class TimeOfDayLocationService
    {
        private readonly ITimeOfDayObservationService observationService;
        private readonly ITimeOfDayProfileService profileService;

        public TimeOfDayLocationService(
            ITimeOfDayObservationService observationService,
            ITimeOfDayProfileService profileService)
        {
            this.observationService = observationService;
            this.profileService = profileService;
        }

        public TimeOfDayLocationAnalysisData BuildAnalysisData(
            TimeOfDayOptions options,
            TimeOfDayLocationReportData data,
            IReadOnlyList<DateOnly> selectedDates,
            List<TimeOfDayWarningDto> warnings)
        {
            var observationResult = BuildObservations(options, data, selectedDates);
            var capacity = ResolveCapacity(options, data, warnings);

            if (!observationResult.HasEligibleDetectors)
            {
                warnings.Add(new TimeOfDayWarningDto
                {
                    Code = options.DataSource == TimeOfDayDataSource.Aggregated ? "NoVehicleDetectors" : "NoTurningMovementDetectors",
                    LocationIdentifier = data.Location.LocationIdentifier,
                    Message = options.DataSource == TimeOfDayDataSource.Aggregated
                        ? $"No vehicle detectors were found for location {data.Location.LocationIdentifier}."
                        : $"No turning-movement vehicle detectors were found for location {data.Location.LocationIdentifier}."
                });
            }

            if (options.DataSource == TimeOfDayDataSource.Aggregated && observationResult.Observations.Count > 0)
            {
                warnings.Add(new TimeOfDayWarningDto
                {
                    Code = "AggregatedMovementDetailLimited",
                    LocationIdentifier = data.Location.LocationIdentifier,
                    Message = "Aggregated detector counts were mapped through detector metadata; movement detail is limited by detector configuration."
                });
            }

            if (observationResult.Observations.Count == 0)
            {
                warnings.Add(new TimeOfDayWarningDto
                {
                    Code = "NoLocationVolumeData",
                    LocationIdentifier = data.Location.LocationIdentifier,
                    Message = $"No usable {options.DataSource} volume data was found for location {data.Location.LocationIdentifier}."
                });
            }

            return new TimeOfDayLocationAnalysisData
            {
                Location = data.Location,
                LocationDescription = data.LocationDescription,
                Observations = observationResult.Observations,
                CapacityVehiclesPerHour = capacity
            };
        }

        public TimeOfDayLocationResult BuildResult(
            TimeOfDayOptions options,
            TimeOfDayLocationAnalysisData data,
            IReadOnlyList<DateOnly> selectedDates,
            TimeOfDayPlanScheduleResult planSchedules,
            List<TimeOfDayWarningDto> warnings)
        {
            var profile = profileService.BuildProfile(
                data.Location.LocationIdentifier,
                string.Empty,
                string.Empty,
                string.Empty,
                data.Observations,
                selectedDates,
                options.BinSizeMinutes);
            var movementProfiles = data.Observations
                .GroupBy(o => new { o.Direction, o.MovementLabel })
                .OrderBy(g => g.Key.Direction)
                .ThenBy(g => g.Key.MovementLabel)
                .Select(g => profileService.BuildProfile(
                    $"{g.Key.Direction} {g.Key.MovementLabel}",
                    g.Key.Direction,
                    g.Key.MovementLabel,
                    g.Key.MovementLabel,
                    g.ToList(),
                    selectedDates,
                    options.BinSizeMinutes))
                .ToList();
            var datesWithData = data.Observations
                .Select(o => o.LocalDate)
                .Distinct()
                .OrderBy(date => date)
                .ToList();
            var missingDates = selectedDates
                .Where(date => !datesWithData.Contains(date))
                .OrderBy(date => date)
                .ToList();
            var daysWithData = datesWithData.Count;

            if (daysWithData == 0)
            {
                warnings.Add(new TimeOfDayWarningDto
                {
                    Code = "NoLocationVolumeData",
                    LocationIdentifier = data.Location.LocationIdentifier,
                    Message = $"Location {data.Location.LocationIdentifier} has no usable volume data for the selected dates."
                });
            }
            else if (missingDates.Count > 0)
            {
                warnings.Add(new TimeOfDayWarningDto
                {
                    Code = "PartialLocationData",
                    LocationIdentifier = data.Location.LocationIdentifier,
                    Message = $"Location {data.Location.LocationIdentifier} has usable volume data for {daysWithData} of {selectedDates.Count} selected dates."
                });
            }

            return new TimeOfDayLocationResult
            {
                LocationIdentifier = data.Location.LocationIdentifier,
                LocationDescription = data.LocationDescription,
                DaysWithData = daysWithData,
                DatesWithData = datesWithData,
                MissingDates = missingDates,
                CoverageFallbackUsed = false,
                Profile = profile,
                MovementProfiles = movementProfiles,
                Summary = BuildLocationSummary(options, data.CapacityVehiclesPerHour, data.Observations, profile, missingDates),
                CurrentPlanSchedule = planSchedules.LocationSchedules.GetValueOrDefault(data.Location.LocationIdentifier) ?? new(),
                DailyPlanSchedules = planSchedules.DailySchedules.GetValueOrDefault(data.Location.LocationIdentifier) ?? new(),
                DataQualityFlag = daysWithData == 0
                    ? "NoData"
                    : missingDates.Count == 0
                        ? "Complete"
                        : "Partial"
            };
        }

        private TimeOfDayObservationBuildResult BuildObservations(
            TimeOfDayOptions options,
            TimeOfDayLocationReportData data,
            IReadOnlyList<DateOnly> selectedDates)
        {
            TimeOfDayObservationBuildResult MapObservations(Location location, IReadOnlyList<DateOnly> dates) =>
                options.DataSource == TimeOfDayDataSource.Aggregated
                ? observationService.BuildAggregatedObservations(
                    location,
                    data.LocationDescription,
                    dates,
                    options.BinSizeMinutes,
                    data.DetectorEventCountAggregations)
                : observationService.BuildIndianaEventObservations(
                    location,
                    data.LocationDescription,
                    dates,
                    options.BinSizeMinutes,
                    data.IndianaEvents);

            var observations = new List<TimeOfDayVolumeObservation>();
            var hasEligibleDetectors = false;
            if (data.LocationsByDate == null)
            {
                var result = MapObservations(data.Location, selectedDates);
                observations.AddRange(result.Observations);
                hasEligibleDetectors = result.HasEligibleDetectors;
            }
            else
            {
                // A configuration may cover several dates; do not remap the same events for each date.
                foreach (var group in data.LocationsByDate.GroupBy(pair => pair.Value,
                    (IEqualityComparer<Location>)ReferenceEqualityComparer.Instance))
                {
                    var result = MapObservations(group.Key, group.Select(pair => pair.Key).ToList());
                    observations.AddRange(result.Observations);
                    hasEligibleDetectors |= result.HasEligibleDetectors;
                }
            }
            return new TimeOfDayObservationBuildResult(observations, hasEligibleDetectors);
        }

        private static double? ResolveCapacity(
            TimeOfDayOptions options,
            TimeOfDayLocationReportData data,
            List<TimeOfDayWarningDto> warnings)
        {
            var capacities = (data.LocationsByDate == null
                    ? new[] { data.Location }
                    : data.LocationsByDate.Values)
                .Select(location => CalculateCapacity(options, location))
                .Distinct()
                .ToList();
            if (capacities.Count > 1)
            {
                warnings.Add(new TimeOfDayWarningDto
                {
                    Code = "CapacityVariesByDate",
                    LocationIdentifier = data.Location.LocationIdentifier,
                    Message = "Configured lane capacity differs across selected dates; capacity percentages are unavailable for the combined profile."
                });
            }

            return capacities.Count == 1 ? capacities[0] : null;
        }

        private static TimeOfDayLocationSummaryDto BuildLocationSummary(
            TimeOfDayOptions options,
            double? capacity,
            IReadOnlyList<TimeOfDayVolumeObservation> observations,
            TimeOfDayProfileDto profile,
            IReadOnlyList<DateOnly> missingDates)
        {
            var peakRaw = profile.Points.Select(p => p.AverageVolume).DefaultIfEmpty(0).Max();
            var peakSmoothed = profile.Points.Select(p => p.SmoothedVolume).DefaultIfEmpty(0).Max();
            var peakHourly = profile.Points.Select(p => p.RollingHourVph ?? 0).DefaultIfEmpty(0).Max();
            var amPeak = profile.Points
                .Where(p => p.Minutes >= TimeOfDayOptions.AmPeakStartMinutes && p.Minutes < TimeOfDayOptions.AmPeakEndMinutes)
                .Select(p => p.SmoothedVolume)
                .DefaultIfEmpty(0)
                .Max();
            var pmPeak = profile.Points
                .Where(p => p.Minutes >= TimeOfDayOptions.PmPeakStartMinutes && p.Minutes < TimeOfDayOptions.PmPeakEndMinutes)
                .Select(p => p.SmoothedVolume)
                .DefaultIfEmpty(0)
                .Max();

            return new TimeOfDayLocationSummaryDto
            {
                PeakRawVolume = peakRaw,
                PeakSmoothedVolume = peakSmoothed,
                PeakHourlyRate = peakHourly > 0 ? peakHourly : null,
                PeakOccupancyPercent = capacity > 0 ? TimeOfDayProfileService.Round(peakSmoothed / capacity.Value * 100) : null,
                AmPeakOccupancyPercent = capacity > 0 ? TimeOfDayProfileService.Round(amPeak / capacity.Value * 100) : null,
                PmPeakOccupancyPercent = capacity > 0 ? TimeOfDayProfileService.Round(pmPeak / capacity.Value * 100) : null,
                CrossTrafficReview = "Cross-traffic review unavailable: no usable cross-traffic data.",
                AmDirectionExceptionMessage = BuildDirectionExceptionMessage(
                    options.AmPrimaryDirections.Count > 0 ? options.AmPrimaryDirections : options.AllDayPrimaryDirections,
                    observations,
                    "AM"),
                PmDirectionExceptionMessage = BuildDirectionExceptionMessage(
                    options.PmPrimaryDirections.Count > 0 ? options.PmPrimaryDirections : options.AllDayPrimaryDirections,
                    observations,
                    "PM"),
                Notes = observations.Count == 0
                    ? "No usable volume observations for the selected dates."
                    : missingDates.Count > 0
                        ? $"Missing volume observations for: {string.Join(", ", missingDates.Select(date => date.ToString("yyyy-MM-dd")))}."
                        : string.Empty
            };
        }

        private static string BuildDirectionExceptionMessage(
            IReadOnlyList<string> requestedDirections,
            IReadOnlyList<TimeOfDayVolumeObservation> observations,
            string period)
        {
            if (requestedDirections.Count == 0)
            {
                return string.Empty;
            }

            var available = observations
                .Select(o => o.Direction)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var missing = requestedDirections
                .Select(TimeOfDayDirectionHelper.NormalizeDirection)
                .Where(d => !available.Contains(d, StringComparer.OrdinalIgnoreCase))
                .ToList();

            return missing.Count == 0
                ? string.Empty
                : $"{period} primary direction data unavailable for {string.Join(", ", missing)}.";
        }

        private static double CalculateCapacity(TimeOfDayOptions options, Location location)
        {
            var overrides = options.DirectionLaneCounts
                .Where(pair => double.IsFinite(pair.Value) && pair.Value > 0)
                .GroupBy(pair => TimeOfDayDirectionHelper.NormalizeDirection(pair.Key), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Value, StringComparer.OrdinalIgnoreCase);
            var laneCount = 0d;
            foreach (var direction in (location.Approaches ?? new List<Approach>())
                .GroupBy(approach => TimeOfDayDirectionHelper.GetDisplayName(approach.DirectionTypeId)))
            {
                if (overrides.TryGetValue(direction.Key, out var overriddenLanes))
                {
                    laneCount += overriddenLanes;
                    continue;
                }

                foreach (var approach in direction)
                {
                    var detectedLanes = (approach.Detectors ?? new List<Detector>())
                        .Where(detector => detector.LaneType == LaneTypes.V && detector.LaneNumber.HasValue)
                        .Select(detector => detector.LaneNumber.Value)
                        .Distinct()
                        .Count();
                    laneCount += detectedLanes > 0 ? detectedLanes : options.ApproachVolumeAssumedLanes;
                }
            }
            return Math.Max(laneCount, 1) * options.LaneCapacityVehiclesPerHour;
        }
    }
}
