#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeOfDay/TimeOfDaySplitPressureService.cs
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

using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public interface ITimeOfDaySplitPressureService
    {
        TimeOfDaySplitPressureDto BuildSplitPressure(
            TimeOfDayOptions options,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            IReadOnlyList<TimeOfDayLocationAnalysisData> locationData,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes);
    }

    public class TimeOfDaySplitPressureService : ITimeOfDaySplitPressureService
    {
        private readonly ITimeOfDayProfileService profileService;

        public TimeOfDaySplitPressureService(ITimeOfDayProfileService profileService)
        {
            this.profileService = profileService;
        }

        public TimeOfDaySplitPressureDto BuildSplitPressure(
            TimeOfDayOptions options,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            IReadOnlyList<TimeOfDayLocationAnalysisData> locationData,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes)
        {
            var primaryDirections = ResolvePrimaryDirections(options, directionalProfiles);
            var crossDirections = ResolveCrossDirections(primaryDirections, directionalProfiles);
            var missingExplicitPrimaryDirections = FindMissingExplicitPrimaryDirections(options, directionalProfiles, locationData);

            if (missingExplicitPrimaryDirections.Count > 0)
            {
                return new TimeOfDaySplitPressureDto
                {
                    PrimaryDirections = primaryDirections,
                    CrossDirections = crossDirections,
                    ThresholdPercentByName = BuildThresholds(options),
                    SummaryText = $"Split-pressure analysis unavailable because primary direction data is unavailable for {string.Join(", ", missingExplicitPrimaryDirections)}."
                };
            }

            var primaryProfile = BuildRepresentativeDirectionProfile(
                "Primary street",
                primaryDirections,
                directionalProfiles,
                locationData,
                selectedDates,
                binSizeMinutes);
            var crossProfile = BuildRepresentativeDirectionProfile(
                "Cross street",
                crossDirections,
                directionalProfiles,
                locationData,
                selectedDates,
                binSizeMinutes);

            var share = BuildCrossTrafficShare(primaryProfile, crossProfile);
            var periodPeaks = BuildPeriodPeaks(primaryProfile, crossProfile, share);
            var peakShare = share
                .Where(s => s.CrossTrafficPercent.HasValue)
                .OrderByDescending(s => s.CrossTrafficPercent)
                .ThenBy(s => s.Minutes)
                .FirstOrDefault();
            var primaryPeak = primaryProfile.Points.OrderByDescending(p => p.AverageVolume).ThenBy(p => p.Minutes).FirstOrDefault();
            var crossPeak = crossProfile.Points.OrderByDescending(p => p.AverageVolume).ThenBy(p => p.Minutes).FirstOrDefault();
            var crossTrafficLocations = BuildCrossTrafficLocations(
                locationData,
                primaryDirections,
                crossDirections,
                selectedDates,
                binSizeMinutes);
            var movementPressures = BuildMovementPressures(
                locationData,
                selectedDates,
                binSizeMinutes);

            return new TimeOfDaySplitPressureDto
            {
                PrimaryDirections = primaryDirections,
                CrossDirections = crossDirections,
                PrimaryProfile = primaryProfile,
                CrossStreetProfile = crossProfile,
                CrossTrafficShare = share,
                ThresholdPercentByName = BuildThresholds(options),
                PeriodPeaks = periodPeaks,
                CrossTrafficLocations = crossTrafficLocations,
                MovementPressures = movementPressures,
                PrimaryPeakVolume = primaryPeak?.AverageVolume,
                PrimaryPeakTime = primaryPeak?.TimeOfDay ?? string.Empty,
                CrossStreetPeakVolume = crossPeak?.AverageVolume,
                CrossStreetPeakTime = crossPeak?.TimeOfDay ?? string.Empty,
                PeakCrossTrafficPercent = peakShare?.CrossTrafficPercent,
                PeakCrossTrafficPercentTime = peakShare?.TimeOfDay ?? string.Empty,
                PrimaryStreetRemainsDominant = (peakShare?.CrossTrafficPercent ?? 0) < 50,
                SummaryText = BuildSummaryText(primaryPeak, crossPeak, peakShare),
                ReviewText = BuildReviewText(
                    peakShare?.CrossTrafficPercent,
                    peakShare?.TimeOfDay,
                    options.SplitReviewThresholdPercent,
                    options.ShoulderReviewThresholdPercent)
            };
        }

        private static List<string> FindMissingExplicitPrimaryDirections(
            TimeOfDayOptions options,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            IReadOnlyList<TimeOfDayLocationAnalysisData> locationData)
        {
            var availableDirections = directionalProfiles
                .Where(profile => profile.Points.Any(point => point.AverageVolume > 0 || point.SmoothedVolume > 0))
                .Select(profile => TimeOfDayDirectionHelper.NormalizeDirection(profile.Direction))
                .Concat(locationData
                    .SelectMany(location => location.Observations)
                    .Select(observation => TimeOfDayDirectionHelper.NormalizeDirection(observation.Direction)))
                .Where(direction => !string.IsNullOrWhiteSpace(direction))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return options.AllDayPrimaryDirections
                .Select(TimeOfDayDirectionHelper.NormalizeDirection)
                .Where(direction => !string.IsNullOrWhiteSpace(direction))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(direction => !availableDirections.Contains(direction, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        private static Dictionary<string, double> BuildThresholds(TimeOfDayOptions options)
        {
            return new Dictionary<string, double>
            {
                ["SplitReview"] = options.SplitReviewThresholdPercent,
                ["ShoulderReview"] = options.ShoulderReviewThresholdPercent
            };
        }

        private static List<string> ResolvePrimaryDirections(
            TimeOfDayOptions options,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles)
        {
            var requested = options.AllDayPrimaryDirections
                .Select(TimeOfDayDirectionHelper.NormalizeDirection)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requested.Count > 0)
            {
                return requested;
            }

            var strongestDirection = directionalProfiles
                .OrderByDescending(p => p.Points.Sum(x => x.AverageVolume))
                .Select(p => p.Direction)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(strongestDirection)
                ? new List<string>()
                : new List<string> { strongestDirection };
        }

        private static List<string> ResolveCrossDirections(
            IReadOnlyList<string> primaryDirections,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles)
        {
            var availableDirections = directionalProfiles
                .Select(p => TimeOfDayDirectionHelper.NormalizeDirection(p.Direction))
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var normalizedPrimaryDirections = primaryDirections
                .Select(TimeOfDayDirectionHelper.NormalizeDirection)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var inferredDirections = InferOppositeAxisDirections(normalizedPrimaryDirections)
                .Where(d => availableDirections.Contains(d, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (inferredDirections.Count > 0)
            {
                return inferredDirections;
            }

            return availableDirections
                .Where(d => !normalizedPrimaryDirections.Contains(d, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        private static IReadOnlyList<string> InferOppositeAxisDirections(IReadOnlyList<string> selectedDirections)
        {
            if (selectedDirections.Contains("Eastbound", StringComparer.OrdinalIgnoreCase) ||
                selectedDirections.Contains("Westbound", StringComparer.OrdinalIgnoreCase))
            {
                return new[] { "Northbound", "Southbound" };
            }

            if (selectedDirections.Contains("Northbound", StringComparer.OrdinalIgnoreCase) ||
                selectedDirections.Contains("Southbound", StringComparer.OrdinalIgnoreCase))
            {
                return new[] { "Eastbound", "Westbound" };
            }

            return Array.Empty<string>();
        }

        private TimeOfDayProfileDto BuildRepresentativeDirectionProfile(
            string label,
            IReadOnlyList<string> directions,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            IReadOnlyList<TimeOfDayLocationAnalysisData> locationData,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes)
        {
            var normalizedDirections = directions
                .Select(TimeOfDayDirectionHelper.NormalizeDirection)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var perLocationProfiles = new List<TimeOfDayProfileDto>();

            foreach (var location in locationData)
            {
                var observations = location.Observations
                    .Where(o => normalizedDirections.Contains(o.Direction, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (observations.Count == 0)
                {
                    continue;
                }

                var profile = profileService.BuildProfile(
                    $"{location.Location.LocationIdentifier} {label}",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    observations,
                    selectedDates,
                    binSizeMinutes);

                if (profile.Points.Any(p => p.AverageVolume > 0 || p.SmoothedVolume > 0))
                {
                    perLocationProfiles.Add(profile);
                }
            }

            if (perLocationProfiles.Count > 0)
            {
                return profileService.MedianProfiles(label, perLocationProfiles);
            }

            return profileService.SumProfiles(
                label,
                directionalProfiles
                    .Where(p => normalizedDirections.Contains(p.Direction, StringComparer.OrdinalIgnoreCase))
                    .ToList());
        }

        private static List<TimeOfDayCrossTrafficSharePointDto> BuildCrossTrafficShare(
            TimeOfDayProfileDto primaryProfile,
            TimeOfDayProfileDto crossProfile)
        {
            var primaryPointsByMinute = primaryProfile.Points
                .GroupBy(p => p.Minutes)
                .ToDictionary(g => g.Key, g => g.First());
            var crossPointsByMinute = crossProfile.Points
                .GroupBy(p => p.Minutes)
                .ToDictionary(g => g.Key, g => g.First());
            var minutes = primaryPointsByMinute.Keys
                .Concat(crossPointsByMinute.Keys)
                .Distinct()
                .OrderBy(m => m)
                .ToList();
            var rows = new List<(int Minutes, double PrimaryVolume, double CrossVolume, double Total)>();

            foreach (var minute in minutes)
            {
                primaryPointsByMinute.TryGetValue(minute, out var primary);
                crossPointsByMinute.TryGetValue(minute, out var cross);
                var primaryVolume = primary?.AverageVolume ?? 0;
                var crossVolume = cross?.AverageVolume ?? 0;
                var total = primaryVolume + crossVolume;

                rows.Add((minute, primaryVolume, crossVolume, total));
            }

            var shareFloor = Math.Max(300d, rows.Select(r => r.Total).DefaultIfEmpty(0).Max() * 0.15d);
            var result = new List<TimeOfDayCrossTrafficSharePointDto>();

            foreach (var row in rows)
            {
                result.Add(new TimeOfDayCrossTrafficSharePointDto
                {
                    TimeOfDay = TimeOfDayProfileService.FormatTime(row.Minutes),
                    Minutes = row.Minutes,
                    PrimaryVolume = TimeOfDayProfileService.Round(row.PrimaryVolume),
                    CrossStreetVolume = TimeOfDayProfileService.Round(row.CrossVolume),
                    TotalVolume = TimeOfDayProfileService.Round(row.Total),
                    CrossTrafficPercent = row.Total >= shareFloor && row.Total > 0
                        ? TimeOfDayProfileService.Round(row.CrossVolume / row.Total * 100)
                        : null
                });
            }

            return result;
        }

        private static List<TimeOfDayPeakEventDto> BuildPeriodPeaks(
            TimeOfDayProfileDto primaryProfile,
            TimeOfDayProfileDto crossProfile,
            IReadOnlyList<TimeOfDayCrossTrafficSharePointDto> share)
        {
            var result = new List<TimeOfDayPeakEventDto>();
            foreach (var period in Periods())
            {
                AddProfilePeak(result, $"{period.Name} primary peak", "Primary", period.Name, primaryProfile, period.Start, period.End);
                AddProfilePeak(result, $"{period.Name} cross-street peak", "CrossStreet", period.Name, crossProfile, period.Start, period.End);

                var sharePeak = share
                    .Where(s => s.Minutes >= period.Start && s.Minutes < period.End && s.CrossTrafficPercent.HasValue)
                    .OrderByDescending(s => s.CrossTrafficPercent)
                    .ThenBy(s => s.Minutes)
                    .FirstOrDefault();

                if (sharePeak != null)
                {
                    result.Add(new TimeOfDayPeakEventDto
                    {
                        Label = $"{period.Name} cross-traffic percent peak",
                        Series = "CrossTrafficPercent",
                        Period = period.Name,
                        TimeOfDay = sharePeak.TimeOfDay,
                        Minutes = sharePeak.Minutes,
                        Value = sharePeak.CrossTrafficPercent!.Value,
                        ValueUnits = "%"
                    });
                }
            }

            return result;
        }

        private static void AddProfilePeak(
            List<TimeOfDayPeakEventDto> result,
            string label,
            string series,
            string period,
            TimeOfDayProfileDto profile,
            int start,
            int end)
        {
            var peak = profile.Points
                .Where(p => p.Minutes >= start && p.Minutes < end)
                .OrderByDescending(p => p.AverageVolume)
                .ThenBy(p => p.Minutes)
                .FirstOrDefault();

            if (peak == null)
            {
                return;
            }

            result.Add(new TimeOfDayPeakEventDto
            {
                Label = label,
                Series = series,
                Period = period,
                TimeOfDay = peak.TimeOfDay,
                Minutes = peak.Minutes,
                Value = peak.AverageVolume,
                ValueUnits = profile.Units
            });
        }

        private List<TimeOfDayCrossTrafficLocationDto> BuildCrossTrafficLocations(
            IReadOnlyList<TimeOfDayLocationAnalysisData> locationData,
            IReadOnlyList<string> primaryDirections,
            IReadOnlyList<string> crossDirections,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes)
        {
            var result = new List<TimeOfDayCrossTrafficLocationDto>();

            foreach (var period in Periods())
            {
                foreach (var location in locationData)
                {
                    var profile = profileService.BuildProfile(
                        $"{location.Location.LocationIdentifier} cross traffic",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        location.Observations
                            .Where(o => crossDirections.Contains(o.Direction, StringComparer.OrdinalIgnoreCase))
                            .ToList(),
                        selectedDates,
                        binSizeMinutes);
                    var peak = profile.Points
                        .Where(p => p.Minutes >= period.Start && p.Minutes < period.End)
                        .OrderByDescending(p => p.AverageVolume)
                        .ThenBy(p => p.Minutes)
                        .FirstOrDefault();

                    if (peak == null || peak.AverageVolume <= 0)
                    {
                        continue;
                    }

                    var observationsAtPeak = location.Observations
                        .Where(o => o.Minutes == peak.Minutes && selectedDates.Contains(o.LocalDate))
                        .ToList();
                    var crossCount = observationsAtPeak
                        .Where(o => crossDirections.Contains(o.Direction, StringComparer.OrdinalIgnoreCase))
                        .Sum(o => o.Count);
                    var totalCount = observationsAtPeak
                        .Where(o => primaryDirections.Contains(o.Direction, StringComparer.OrdinalIgnoreCase)
                            || crossDirections.Contains(o.Direction, StringComparer.OrdinalIgnoreCase))
                        .Sum(o => o.Count);

                    // Use the same dates and bin for both counts. A common daily-average
                    // and hourly-rate conversion cancels out of this location's share.

                    result.Add(new TimeOfDayCrossTrafficLocationDto
                    {
                        LocationIdentifier = location.Location.LocationIdentifier,
                        LocationDescription = location.LocationDescription,
                        Period = period.Name,
                        PeakTime = peak.TimeOfDay,
                        Minutes = peak.Minutes,
                        TotalVehiclesPerHour = peak.AverageVolume,
                        PercentOfCrossTraffic = totalCount > 0
                            ? TimeOfDayProfileService.Round(crossCount / totalCount * 100)
                            : null
                    });
                }
            }

            return result
                .OrderBy(r => r.Period)
                .ThenByDescending(r => r.TotalVehiclesPerHour)
                .ToList();
        }

        private List<TimeOfDayMovementPressureDto> BuildMovementPressures(
            IReadOnlyList<TimeOfDayLocationAnalysisData> locationData,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes)
        {
            var result = new List<TimeOfDayMovementPressureDto>();
            var movementNames = new[] { "Left", "Thru", "Right" };

            foreach (var period in Periods().Where(p => p.Name is "AM" or "PM"))
            {
                foreach (var location in locationData)
                {
                    foreach (var movement in movementNames)
                    {
                        var profile = profileService.BuildProfile(
                            $"{location.Location.LocationIdentifier} {movement}",
                            string.Empty,
                            movement,
                            movement,
                            location.Observations
                                .Where(o => string.Equals(o.MovementLabel, movement, StringComparison.OrdinalIgnoreCase))
                                .ToList(),
                            selectedDates,
                            binSizeMinutes);
                        var peak = profile.Points
                            .Where(p => p.Minutes >= period.Start && p.Minutes < period.End)
                            .OrderByDescending(p => p.AverageVolume)
                            .ThenBy(p => p.Minutes)
                            .FirstOrDefault();

                        if (peak == null || peak.AverageVolume <= 0)
                        {
                            continue;
                        }

                        result.Add(new TimeOfDayMovementPressureDto
                        {
                            Period = period.Name,
                            LocationIdentifier = location.Location.LocationIdentifier,
                            Movement = movement,
                            MovementLabel = movement,
                            PeakTime = peak.TimeOfDay,
                            Volume = peak.AverageVolume
                        });
                    }
                }
            }

            return result
                .OrderBy(r => r.Period)
                .ThenByDescending(r => r.Volume)
                .ToList();
        }

        private static string BuildReviewText(
            double? peakCrossTrafficPercent,
            string peakTime,
            double splitReviewThresholdPercent,
            double shoulderReviewThresholdPercent)
        {
            if (!peakCrossTrafficPercent.HasValue)
            {
                return string.Empty;
            }

            if (peakCrossTrafficPercent.Value >= shoulderReviewThresholdPercent)
            {
                return $"Cross traffic reaches {peakCrossTrafficPercent.Value:0.#}% at {peakTime}; review shoulder timing or special split treatment.";
            }

            if (peakCrossTrafficPercent.Value >= splitReviewThresholdPercent)
            {
                return $"Cross traffic reaches {peakCrossTrafficPercent.Value:0.#}% at {peakTime}; review split allocation during this period.";
            }

            return $"Cross traffic peaks at {peakCrossTrafficPercent.Value:0.#}% at {peakTime}; primary street remains dominant.";
        }

        private static string BuildSummaryText(
            TimeOfDayProfilePointDto primaryPeak,
            TimeOfDayProfilePointDto crossPeak,
            TimeOfDayCrossTrafficSharePointDto peakShare)
        {
            if (primaryPeak == null && crossPeak == null)
            {
                return "Split-pressure summary unavailable because no directional profile was found.";
            }

            return $"Primary peak {primaryPeak?.TimeOfDay ?? "unavailable"}; cross-street peak {crossPeak?.TimeOfDay ?? "unavailable"}; peak cross-traffic share {peakShare?.CrossTrafficPercent?.ToString("0.#") ?? "unavailable"}%.";
        }

        private static IReadOnlyList<(string Name, int Start, int End)> Periods()
        {
            return new List<(string Name, int Start, int End)>
            {
                ("AM", 5 * 60, 10 * 60),
                ("Midday", 10 * 60, 15 * 60),
                ("PM", 15 * 60, 19 * 60)
            };
        }
    }
}
