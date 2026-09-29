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

using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public class TimeOfDayService
    {
        private readonly TimeOfDayLocationService locationService;
        private readonly ITimeOfDayProfileService profileService;
        private readonly ITimeOfDayRecommendationService recommendationService;
        private readonly ITimeOfDayPlanScheduleService planScheduleService;
        private readonly ITimeOfDayPlanProfileService planProfileService;
        private readonly ITimeOfDaySplitPressureService splitPressureService;

        public TimeOfDayService(
            TimeOfDayLocationService locationService,
            ITimeOfDayProfileService profileService,
            ITimeOfDayRecommendationService recommendationService,
            ITimeOfDayPlanScheduleService planScheduleService,
            ITimeOfDayPlanProfileService planProfileService,
            ITimeOfDaySplitPressureService splitPressureService)
        {
            this.locationService = locationService;
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
                reportData,
                selectedDates,
                options.BinSizeMinutes);
            var locationData = reportData
                .Select(data => locationService.BuildAnalysisData(options, data, selectedDates, warnings))
                .ToList();

            var usableLocationData = locationData
                .Where(d => d.Observations.Count > 0)
                .ToList();
            var result = new TimeOfDayResult
            {
                LocationIdentifiers = locationIdentifiers.ToList(),
                SelectedDates = selectedDates.ToList(),
                BinSizeMinutes = options.BinSizeMinutes,
                DataSource = options.DataSource.ToString(),
                PlanComparison = planScheduleResult.Comparison,
                Locations = locationData
                    .Select(data => locationService.BuildResult(options, data, selectedDates, planScheduleResult, warnings))
                    .ToList(),
                Warnings = warnings,
                Notes = "Time-of-day analysis is based only on the submitted local calendar dates."
            };

            if (usableLocationData.Count == 0)
            {
                warnings.Add(new TimeOfDayWarningDto
                {
                    Code = "NoUsableVolumeData",
                    Message = "No usable volume data was found for any selected location and date."
                });

                result.Notes = "No volume profile could be built from the selected data source.";
                return result;
            }

            var corridorProfile = profileService.BuildRepresentativeProfile(
                "Corridor",
                usableLocationData,
                selectedDates,
                options.BinSizeMinutes,
                data => data.Observations);
            var directionalProfiles = usableLocationData
                .SelectMany(d => d.Observations.Select(o => o.Direction))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .Select(direction => profileService.BuildRepresentativeProfile(
                    direction,
                    usableLocationData,
                    selectedDates,
                    options.BinSizeMinutes,
                    data => data.Observations
                        .Where(o => string.Equals(o.Direction, direction, StringComparison.OrdinalIgnoreCase)),
                    direction))
                .Where(p => p.Points.Any(point => point.AverageVolume > 0 || point.SmoothedVolume > 0))
                .ToList();
            AddPrimaryDirectionWarnings(options, directionalProfiles, warnings);
            result.Recommendation = recommendationService.BuildRecommendation(
                options,
                corridorProfile,
                directionalProfiles,
                selectedDates[0]);
            result.PlanProfile = planProfileService.BuildPlanProfile(
                corridorProfile,
                directionalProfiles,
                result.Locations);
            result.SplitPressure = splitPressureService.BuildSplitPressure(
                options,
                directionalProfiles,
                usableLocationData,
                selectedDates,
                options.BinSizeMinutes);

            foreach (var location in result.Locations)
            {
                var peak = result.SplitPressure.CrossTrafficLocations
                    .Where(row => row.LocationIdentifier == location.LocationIdentifier && row.PercentOfCrossTraffic.HasValue)
                    .OrderByDescending(row => row.PercentOfCrossTraffic)
                    .ThenBy(row => row.Minutes)
                    .FirstOrDefault();
                if (peak != null)
                {
                    location.Summary.CrossTrafficReview = $"{peak.Period}: " + TimeOfDaySplitPressureService.BuildReviewText(
                        peak.PercentOfCrossTraffic, peak.PeakTime,
                        options.SplitReviewThresholdPercent, options.ShoulderReviewThresholdPercent);
                }
                else if (result.SplitPressure.CrossTrafficLocations.Count == 0 && !string.IsNullOrWhiteSpace(result.SplitPressure.SummaryText))
                {
                    location.Summary.CrossTrafficReview = result.SplitPressure.SummaryText;
                }
            }

            return result;
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
            var missingDirections = TimeOfDayDirectionHelper.FindMissingDirections(
                requestedDirections,
                directionalProfiles
                    .Where(profile => profile.Points.Any(point => point.AverageVolume > 0 || point.SmoothedVolume > 0))
                    .Select(profile => profile.Direction));

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
    }
}
