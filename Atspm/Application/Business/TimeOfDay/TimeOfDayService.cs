#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeOfDay/TimeOfDayService.cs
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

using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public class TimeOfDayLocationAnalysisData
    {
        public Location Location { get; init; }
        public string LocationDescription { get; init; } = string.Empty;
        public List<TimeOfDayVolumeObservation> Observations { get; init; } = new();
        public List<Plan> CurrentPlanSchedule { get; set; } = new();
        public bool HasCurrentPlanData { get; set; }
    }

    public class TimeOfDayLocationReportData
    {
        public Location Location { get; init; }
        public string LocationDescription { get; init; } = string.Empty;
        public List<IndianaEvent> IndianaEvents { get; } = new();
        public List<IndianaEvent> IndianaPlanEvents { get; } = new();
        public List<DetectorEventCountAggregation> DetectorEventCountAggregations { get; } = new();
        public List<SignalTimingPlan> SignalTimingPlans { get; } = new();
    }

    public class TimeOfDayService
    {
        private readonly ITimeOfDayObservationService observationService;
        private readonly ITimeOfDayProfileService profileService;
        private readonly ITimeOfDayRecommendationService recommendationService;
        private readonly ITimeOfDayPlanScheduleService planScheduleService;
        private readonly ITimeOfDayPlanProfileService planProfileService;
        private readonly ITimeOfDaySplitPressureService splitPressureService;

        public TimeOfDayService(
            ITimeOfDayObservationService observationService,
            ITimeOfDayProfileService profileService,
            ITimeOfDayRecommendationService recommendationService,
            ITimeOfDayPlanScheduleService planScheduleService,
            ITimeOfDayPlanProfileService planProfileService,
            ITimeOfDaySplitPressureService splitPressureService)
        {
            this.observationService = observationService;
            this.profileService = profileService;
            this.recommendationService = recommendationService;
            this.planScheduleService = planScheduleService;
            this.planProfileService = planProfileService;
            this.splitPressureService = splitPressureService;
        }

        public TimeOfDayResult GetChartData(
            TimeOfDayOptions options,
            IReadOnlyList<string> locationIdentifiers,
            IReadOnlyList<DateOnly> selectedDates,
            IReadOnlyList<TimeOfDayLocationReportData> reportData,
            List<TimeOfDayWarningDto> warnings)
        {
            var planScheduleResult = planScheduleService.BuildCurrentSchedules(
                options.DataSource,
                reportData,
                selectedDates,
                options.BinSizeMinutes);
            var locationData = reportData
                .Select(data => BuildLocationAnalysisData(options, data, selectedDates, warnings))
                .ToList();

            foreach (var data in locationData)
            {
                data.CurrentPlanSchedule = planScheduleResult.LocationSchedules.GetValueOrDefault(data.Location.LocationIdentifier) ?? new();
                data.HasCurrentPlanData = planScheduleResult.HasPlanDataByLocation.GetValueOrDefault(data.Location.LocationIdentifier);
            }

            var usableLocationData = locationData
                .Where(d => d.Observations.Count > 0)
                .ToList();
            var locationResults = BuildLocationResults(
                options,
                locationData,
                selectedDates,
                warnings);

            if (usableLocationData.Count == 0)
            {
                warnings.Add(new TimeOfDayWarningDto
                {
                    Code = "NoUsableVolumeData",
                    Message = "No usable volume data was found for any selected location and date."
                });

                return new TimeOfDayResult
                {
                    LocationIdentifiers = locationIdentifiers.ToList(),
                    SelectedDates = selectedDates.ToList(),
                    BinSizeMinutes = options.BinSizeMinutes,
                    DataSource = options.DataSource.ToString(),
                    PlanComparison = planScheduleResult.Comparison,
                    Locations = locationResults,
                    Warnings = warnings,
                    Notes = "No volume profile could be built from the selected data source."
                };
            }

            var corridorProfile = BuildRepresentativeProfile(
                "Corridor",
                string.Empty,
                string.Empty,
                string.Empty,
                usableLocationData,
                selectedDates,
                options.BinSizeMinutes,
                data => data.Observations);
            var directionalProfiles = usableLocationData
                .SelectMany(d => d.Observations.Select(o => o.Direction))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .Select(direction => BuildRepresentativeProfile(
                    direction,
                    direction,
                    string.Empty,
                    string.Empty,
                    usableLocationData,
                    selectedDates,
                    options.BinSizeMinutes,
                    data => data.Observations
                        .Where(o => string.Equals(o.Direction, direction, StringComparison.OrdinalIgnoreCase))))
                .Where(p => p.Points.Any(point => point.AverageVolume > 0 || point.SmoothedVolume > 0))
                .ToList();
            AddPrimaryDirectionWarnings(options, directionalProfiles, warnings);
            var recommendation = recommendationService.BuildRecommendation(
                options,
                corridorProfile,
                directionalProfiles,
                selectedDates[0]);
            var planProfile = planProfileService.BuildPlanProfile(
                corridorProfile,
                directionalProfiles,
                locationResults);
            var splitPressure = splitPressureService.BuildSplitPressure(
                options,
                directionalProfiles,
                usableLocationData,
                selectedDates,
                options.BinSizeMinutes);

            return new TimeOfDayResult
            {
                LocationIdentifiers = locationIdentifiers.ToList(),
                SelectedDates = selectedDates.ToList(),
                BinSizeMinutes = options.BinSizeMinutes,
                DataSource = options.DataSource.ToString(),
                Recommendation = recommendation,
                PlanProfile = planProfile,
                SplitPressure = splitPressure,
                PlanComparison = planScheduleResult.Comparison,
                Locations = locationResults,
                Warnings = warnings,
                Notes = "Time-of-day analysis is based only on the submitted local calendar dates."
            };
        }

        private TimeOfDayLocationAnalysisData BuildLocationAnalysisData(
            TimeOfDayOptions options,
            TimeOfDayLocationReportData data,
            IReadOnlyList<DateOnly> selectedDates,
            List<TimeOfDayWarningDto> warnings)
        {
            var observationResult = options.DataSource == TimeOfDayDataSource.Aggregated
                ? observationService.BuildAggregatedObservations(
                    data.Location,
                    data.LocationDescription,
                    selectedDates,
                    options.BinSizeMinutes,
                    data.DetectorEventCountAggregations)
                : observationService.BuildIndianaEventObservations(
                    data.Location,
                    data.LocationDescription,
                    selectedDates,
                    options.BinSizeMinutes,
                    data.IndianaEvents);

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
                Observations = observationResult.Observations
            };
        }

        private List<TimeOfDayLocationResult> BuildLocationResults(
            TimeOfDayOptions options,
            IReadOnlyList<TimeOfDayLocationAnalysisData> locationData,
            IReadOnlyList<DateOnly> selectedDates,
            List<TimeOfDayWarningDto> warnings)
        {
            var results = new List<TimeOfDayLocationResult>();

            foreach (var data in locationData)
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

                results.Add(new TimeOfDayLocationResult
                {
                    LocationIdentifier = data.Location.LocationIdentifier,
                    LocationDescription = data.LocationDescription,
                    DaysWithData = daysWithData,
                    DatesWithData = datesWithData,
                    MissingDates = missingDates,
                    CoverageFallbackUsed = false,
                    Profile = profile,
                    MovementProfiles = movementProfiles,
                    Summary = BuildLocationSummary(options, data.Location, data.Observations, profile, missingDates),
                    CurrentPlanSchedule = data.CurrentPlanSchedule,
                    DataQualityFlag = daysWithData == 0
                        ? "NoData"
                        : missingDates.Count == 0
                            ? "Complete"
                            : "Partial"
                });
            }

            return results;
        }

        private TimeOfDayProfileDto BuildRepresentativeProfile(
            string label,
            string direction,
            string movement,
            string movementLabel,
            IReadOnlyList<TimeOfDayLocationAnalysisData> locationData,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes,
            Func<TimeOfDayLocationAnalysisData, IEnumerable<TimeOfDayVolumeObservation>> observationSelector)
        {
            var perLocationProfiles = new List<TimeOfDayProfileDto>();

            foreach (var location in locationData)
            {
                var observations = observationSelector(location).ToList();
                if (observations.Count == 0)
                {
                    continue;
                }

                var profile = profileService.BuildProfile(
                    $"{location.Location.LocationIdentifier} {label}",
                    direction,
                    movement,
                    movementLabel,
                    observations,
                    selectedDates,
                    binSizeMinutes);

                if (profile.Points.Any(p => p.AverageVolume > 0 || p.SmoothedVolume > 0))
                {
                    perLocationProfiles.Add(profile);
                }
            }

            return profileService.MedianProfiles(
                label,
                perLocationProfiles,
                direction,
                movement,
                movementLabel);
        }

        private static TimeOfDayLocationSummaryDto BuildLocationSummary(
            TimeOfDayOptions options,
            Location location,
            IReadOnlyList<TimeOfDayVolumeObservation> observations,
            TimeOfDayProfileDto profile,
            IReadOnlyList<DateOnly> missingDates)
        {
            var capacity = CalculateCapacity(options, location);
            var peakRaw = profile.Points.Select(p => p.AverageVolume).DefaultIfEmpty(0).Max();
            var peakSmoothed = profile.Points.Select(p => p.SmoothedVolume).DefaultIfEmpty(0).Max();
            var peakHourly = profile.Points.Select(p => p.RollingHourVph ?? 0).DefaultIfEmpty(0).Max();
            var amPeak = profile.Points
                .Where(p => p.Minutes >= 5 * 60 && p.Minutes < 10 * 60)
                .Select(p => p.SmoothedVolume)
                .DefaultIfEmpty(0)
                .Max();
            var pmPeak = profile.Points
                .Where(p => p.Minutes >= 15 * 60 && p.Minutes < 19 * 60)
                .Select(p => p.SmoothedVolume)
                .DefaultIfEmpty(0)
                .Max();

            return new TimeOfDayLocationSummaryDto
            {
                PeakRawVolume = peakRaw,
                PeakSmoothedVolume = peakSmoothed,
                PeakHourlyRate = peakHourly > 0 ? peakHourly : null,
                PeakOccupancyPercent = capacity > 0 ? TimeOfDayProfileService.Round(peakSmoothed / capacity * 100) : null,
                AmPeakOccupancyPercent = capacity > 0 ? TimeOfDayProfileService.Round(amPeak / capacity * 100) : null,
                PmPeakOccupancyPercent = capacity > 0 ? TimeOfDayProfileService.Round(pmPeak / capacity * 100) : null,
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

        private static void AddPrimaryDirectionWarnings(
            TimeOfDayOptions options,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            List<TimeOfDayWarningDto> warnings)
        {
            var amDirections = options.AmPrimaryDirections.Count > 0
                ? options.AmPrimaryDirections
                : options.AllDayPrimaryDirections;
            var pmDirections = options.PmPrimaryDirections.Count > 0
                ? options.PmPrimaryDirections
                : options.AllDayPrimaryDirections;

            AddPrimaryDirectionWarning("AM", amDirections, directionalProfiles, warnings);
            AddPrimaryDirectionWarning("PM", pmDirections, directionalProfiles, warnings);
            AddPrimaryDirectionWarning("Split-pressure", options.AllDayPrimaryDirections, directionalProfiles, warnings);
        }

        private static void AddPrimaryDirectionWarning(
            string period,
            IReadOnlyList<string> requestedDirections,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            List<TimeOfDayWarningDto> warnings)
        {
            var availableDirections = directionalProfiles
                .Where(profile => profile.Points.Any(point => point.AverageVolume > 0 || point.SmoothedVolume > 0))
                .Select(profile => TimeOfDayDirectionHelper.NormalizeDirection(profile.Direction))
                .Where(direction => !string.IsNullOrWhiteSpace(direction))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var missingDirections = requestedDirections
                .Select(TimeOfDayDirectionHelper.NormalizeDirection)
                .Where(direction => !string.IsNullOrWhiteSpace(direction))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(direction => !availableDirections.Contains(direction, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (missingDirections.Count == 0)
            {
                return;
            }

            warnings.Add(new TimeOfDayWarningDto
            {
                Code = "PrimaryDirectionDataUnavailable",
                Message = $"{period} primary direction data is unavailable for {string.Join(", ", missingDirections)}."
            });
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
            var laneCount = options.DirectionLaneCounts
                .Where(kvp => location.Approaches
                    .Select(a => TimeOfDayDirectionHelper.GetDisplayName(a.DirectionTypeId))
                    .Contains(TimeOfDayDirectionHelper.NormalizeDirection(kvp.Key), StringComparer.OrdinalIgnoreCase))
                .Sum(kvp => kvp.Value);

            if (laneCount <= 0)
            {
                laneCount = TimeOfDayDetectorHelper.GetVehicleDetectors(location)
                    .Where(d => d.LaneNumber.HasValue)
                    .Select(d => new { d.Approach.DirectionTypeId, d.LaneNumber })
                    .Distinct()
                    .Count();
            }

            if (laneCount <= 0)
            {
                laneCount = options.ApproachVolumeAssumedLanes;
            }

            return Math.Max(laneCount, 1) * options.LaneCapacityVehiclesPerHour;
        }
    }
}
