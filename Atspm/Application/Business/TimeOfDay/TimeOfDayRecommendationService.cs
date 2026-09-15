#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeOfDay/TimeOfDayRecommendationService.cs
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
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public interface ITimeOfDayRecommendationService
    {
        TimeOfDayRecommendationDto BuildRecommendation(
            TimeOfDayOptions options,
            TimeOfDayProfileDto corridorProfile,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            DateOnly representativeDate);
    }

    public class TimeOfDayRecommendationService : ITimeOfDayRecommendationService
    {
        private const string AlgorithmVersion = "tod-v2";
        private const string ThresholdConfigurationName = "Configured thresholds";

        private readonly ITimeOfDayProfileService profileService;

        public TimeOfDayRecommendationService(ITimeOfDayProfileService profileService)
        {
            this.profileService = profileService;
        }

        public TimeOfDayRecommendationDto BuildRecommendation(
            TimeOfDayOptions options,
            TimeOfDayProfileDto corridorProfile,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            DateOnly representativeDate)
        {
            if (corridorProfile.Points.Count == 0 || corridorProfile.Points.All(p => p.SmoothedVolume <= 0))
            {
                return new TimeOfDayRecommendationDto
                {
                    AlgorithmVersion = AlgorithmVersion,
                    ThresholdConfigurationName = ThresholdConfigurationName,
                    SummaryText = "Recommended schedule unavailable because no usable volume profile was found."
                };
            }

            var amRequestedDirections = options.AmPrimaryDirections.Count > 0
                ? options.AmPrimaryDirections
                : options.AllDayPrimaryDirections;
            var pmRequestedDirections = options.PmPrimaryDirections.Count > 0
                ? options.PmPrimaryDirections
                : options.AllDayPrimaryDirections;
            var unavailableDirectionMessages = new List<string>();
            AddUnavailableDirectionMessage("AM", amRequestedDirections, directionalProfiles, unavailableDirectionMessages);
            AddUnavailableDirectionMessage("PM", pmRequestedDirections, directionalProfiles, unavailableDirectionMessages);

            if (unavailableDirectionMessages.Count > 0)
            {
                return new TimeOfDayRecommendationDto
                {
                    AlgorithmVersion = AlgorithmVersion,
                    ThresholdConfigurationName = ThresholdConfigurationName,
                    SummaryText = $"Recommended schedule unavailable because primary direction data is unavailable for {string.Join("; ", unavailableDirectionMessages)}."
                };
            }

            var amProfile = SelectProfile(
                amRequestedDirections,
                directionalProfiles,
                corridorProfile,
                "AM primary");
            var pmProfile = SelectProfile(
                pmRequestedDirections,
                directionalProfiles,
                corridorProfile,
                "PM primary");

            var maxAmEnd = ParseTimeOrDefault(options.MaxAmEndTime, 10 * 60);
            var maxPmEnd = ParseTimeOrDefault(options.MaxPmEndTime, 20 * 60);
            var freeFallback = ParseTimeOrDefault(options.FreeFallbackTime, 23 * 60 + 30);
            var binSize = InferBinSize(corridorProfile);

            var amPeak = FindPeak(amProfile, 5 * 60, 10 * 60);
            var pmPeak = FindPeak(pmProfile, 14 * 60, 19 * 60);
            var dailyPeak = corridorProfile.Points.Max(p => p.SmoothedVolume);
            var baseline = Percentile(corridorProfile.Points.Select(p => p.SmoothedVolume).ToList(), 0.15);
            var amPeakValue = amPeak != null
                ? FindMaxSmoothed(amProfile, 5 * 60, 12 * 60, dailyPeak)
                : dailyPeak;
            var pmPeakValue = pmPeak != null
                ? FindMaxSmoothed(pmProfile, 12 * 60, 19 * 60, dailyPeak)
                : dailyPeak;

            var amEntryThreshold = baseline + (amPeakValue - baseline) * options.AmEntryPctOfPeak;
            var amExitThreshold = baseline + (amPeakValue - baseline) * options.AmExitPctOfPeak;
            var pmEntryThreshold = baseline + (pmPeakValue - baseline) * options.PmEntryPctOfPeak;
            var pmExitThreshold = baseline + (pmPeakValue - baseline) * options.PmExitPctOfPeak;
            var freeThreshold = Math.Max(
                dailyPeak * options.FreeEntryPctOfDailyPeak,
                baseline + (dailyPeak - baseline) * options.FreeEntryPctOfDynamicRange);

            var amEntry = FindFirstSustained(
                amProfile,
                4 * 60,
                amPeak?.Minutes ?? 10 * 60,
                amEntryThreshold,
                options.EntrySustainedBins,
                true);
            var amExit = amPeak != null
                ? FindLastSustainedAbove(
                    amProfile,
                    amPeak.Minutes,
                    maxAmEnd,
                    amExitThreshold,
                    options.EntrySustainedBins)
                : null;

            if (!amExit.HasValue && amPeak != null)
            {
                amExit = FindValley(amProfile, amPeak.Minutes, maxAmEnd)?.Minutes;
            }

            if (amEntry.HasValue && amExit.HasValue && amExit.Value <= amEntry.Value)
            {
                amExit = FindValley(amProfile, amEntry.Value + 60, 12 * 60)?.Minutes;
            }

            if (amEntry.HasValue && !amExit.HasValue)
            {
                amExit = maxAmEnd;
            }

            if (amExit.HasValue && amExit.Value > maxAmEnd)
            {
                amExit = maxAmEnd;
            }

            if (amEntry.HasValue && amExit.HasValue && amEntry.Value >= amExit.Value)
            {
                amEntry = FindFirstSustained(
                    amProfile,
                    5 * 60,
                    maxAmEnd - 60,
                    amEntryThreshold,
                    options.EntrySustainedBins,
                    true) ?? 6 * 60;
            }

            amEntry ??= 6 * 60;
            amExit ??= maxAmEnd;

            var pmEntry = FindFirstSustained(
                pmProfile,
                Math.Max(amExit.Value, 14 * 60),
                pmPeak?.Minutes ?? 19 * 60,
                pmEntryThreshold,
                options.EntrySustainedBins,
                true);
            var pmExit = pmPeak != null
                ? FindLastSustainedAbove(
                    pmProfile,
                    pmPeak.Minutes,
                    maxPmEnd,
                    pmExitThreshold,
                    options.EntrySustainedBins)
                : null;

            if (pmEntry.HasValue && pmExit.HasValue && pmExit.Value <= pmEntry.Value)
            {
                pmExit = FindValley(pmProfile, pmEntry.Value + 60, 22 * 60)?.Minutes;
            }

            if (!pmExit.HasValue)
            {
                pmExit = FindValley(pmProfile, 16 * 60, maxPmEnd)?.Minutes ?? maxPmEnd;
            }

            if (pmExit.HasValue && pmExit.Value > maxPmEnd)
            {
                pmExit = maxPmEnd;
            }

            if (pmEntry.HasValue && pmExit.HasValue && pmEntry.Value >= pmExit.Value)
            {
                pmEntry = FindFirstSustained(
                    pmProfile,
                    14 * 60,
                    maxPmEnd - 60,
                    pmEntryThreshold,
                    options.EntrySustainedBins,
                    true);

                if (pmEntry.HasValue && pmEntry.Value >= pmExit.Value)
                {
                    pmEntry = Math.Max(14 * 60, maxPmEnd - 180);
                }
            }

            pmEntry ??= 14 * 60;
            pmExit ??= maxPmEnd;

            var middayValley = amPeak != null && pmPeak != null
                ? FindValley(
                    corridorProfile,
                    Math.Max(amPeak.Minutes, 9 * 60 + 30),
                    Math.Min(pmPeak.Minutes, 16 * 60))
                : null;
            int? middayStart = amExit;
            int? middayEnd = pmEntry;

            if (middayValley != null)
            {
                if (!middayStart.HasValue)
                {
                    middayStart = Math.Max(9 * 60 + 30, Math.Min(middayValley.Minutes, 13 * 60));
                }

                if (pmEntry.Value < middayValley.Minutes)
                {
                    pmEntry = Math.Max(14 * 60, middayValley.Minutes);
                    middayEnd = pmEntry;
                }
            }

            if (!middayStart.HasValue && amPeak != null)
            {
                middayStart = FindValley(corridorProfile, amPeak.Minutes, 14 * 60)?.Minutes;
            }

            if (middayStart.HasValue && middayStart.Value < 9 * 60)
            {
                middayStart = 9 * 60;
            }

            if (pmEntry.Value < 14 * 60)
            {
                pmEntry = 14 * 60;
                middayEnd = pmEntry;
            }

            middayStart = amExit;
            middayEnd = pmEntry;

            if (middayStart.HasValue && middayEnd.HasValue && middayEnd.Value <= middayStart.Value)
            {
                middayStart = null;
                middayEnd = null;
            }

            var eveningStart = pmExit.Value;
            var freeStartFloor = Math.Max(pmExit.Value, 19 * 60);
            var freeStart = FindFirstSustained(
                corridorProfile,
                freeStartFloor,
                23 * 60 + 30,
                freeThreshold,
                options.FreeSustainedBins,
                false);

            if (freeStart.HasValue && freeStart.Value <= eveningStart)
            {
                freeStart = null;
            }

            freeStart ??= freeFallback;

            var boundaries = NormalizeBoundaries(
                new[] { amEntry.Value, amExit.Value, pmEntry.Value, pmExit.Value, freeStart.Value },
                binSize);
            var start = representativeDate.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);
            var schedule = new List<Plan>();

            AddPlan(schedule, "254", start, start.AddMinutes(boundaries[0]));
            AddPlan(schedule, "1", start.AddMinutes(boundaries[0]), start.AddMinutes(boundaries[1]));
            AddPlan(schedule, "7", start.AddMinutes(boundaries[1]), start.AddMinutes(boundaries[2]));
            AddPlan(schedule, "13", start.AddMinutes(boundaries[2]), start.AddMinutes(boundaries[3]));
            AddPlan(schedule, "7", start.AddMinutes(boundaries[3]), start.AddMinutes(boundaries[4]));
            AddPlan(schedule, "254", start.AddMinutes(boundaries[4]), end);

            return new TimeOfDayRecommendationDto
            {
                RecommendedSchedule = schedule,
                AmPeakTime = amPeak?.TimeOfDay ?? string.Empty,
                MiddayValleyTime = middayValley?.TimeOfDay ?? FindValley(corridorProfile, boundaries[1], boundaries[2])?.TimeOfDay ?? string.Empty,
                PmPeakTime = pmPeak?.TimeOfDay ?? string.Empty,
                AlgorithmVersion = AlgorithmVersion,
                ThresholdConfigurationName = ThresholdConfigurationName,
                SummaryText = $"Recommended TOD schedule uses {AlgorithmVersion} with AM peak {amPeak?.TimeOfDay ?? "unavailable"} and PM peak {pmPeak?.TimeOfDay ?? "unavailable"}."
            };
        }

        private TimeOfDayProfileDto SelectProfile(
            IReadOnlyList<string> requestedDirections,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            TimeOfDayProfileDto fallback,
            string label)
        {
            var normalized = requestedDirections
                .Select(TimeOfDayDirectionHelper.NormalizeDirection)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalized.Count == 0)
            {
                return fallback;
            }

            var matches = directionalProfiles
                .Where(p => normalized.Contains(p.Direction, StringComparer.OrdinalIgnoreCase))
                .ToList();

            return matches.Count == 0
                ? new TimeOfDayProfileDto { Label = label }
                : profileService.SumProfiles(label, matches);
        }

        private static void AddUnavailableDirectionMessage(
            string period,
            IReadOnlyList<string> requestedDirections,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            List<string> messages)
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

            if (missingDirections.Count > 0)
            {
                messages.Add($"{period}: {string.Join(", ", missingDirections)}");
            }
        }

        private static TimeOfDayProfilePointDto FindPeak(TimeOfDayProfileDto profile, int startMinutes, int endMinutes)
        {
            return profile.Points
                .Where(p => p.Minutes >= startMinutes && p.Minutes <= endMinutes)
                .OrderByDescending(p => p.SmoothedVolume)
                .ThenBy(p => p.Minutes)
                .FirstOrDefault();
        }

        private static TimeOfDayProfilePointDto FindValley(TimeOfDayProfileDto profile, int startMinutes, int endMinutes)
        {
            return profile.Points
                .Where(p => p.Minutes >= startMinutes && p.Minutes <= endMinutes)
                .OrderBy(p => p.SmoothedVolume)
                .ThenBy(p => p.Minutes)
                .FirstOrDefault();
        }

        private static double FindMaxSmoothed(
            TimeOfDayProfileDto profile,
            int startMinutes,
            int endMinutes,
            double fallback)
        {
            var values = profile.Points
                .Where(p => p.Minutes >= startMinutes && p.Minutes <= endMinutes)
                .Select(p => p.SmoothedVolume)
                .ToList();

            return values.Count > 0 ? values.Max() : fallback;
        }

        private static int? FindFirstSustained(
            TimeOfDayProfileDto profile,
            int startMinutes,
            int endMinutes,
            double threshold,
            int sustainedBins,
            bool aboveThreshold)
        {
            var points = profile.Points
                .Where(p => p.Minutes >= startMinutes && p.Minutes <= endMinutes)
                .OrderBy(p => p.Minutes)
                .ToList();

            for (var i = 0; i < points.Count; i++)
            {
                if (i + sustainedBins > points.Count)
                {
                    break;
                }

                var sustained = points
                    .Skip(i)
                    .Take(sustainedBins)
                    .All(p => aboveThreshold ? p.SmoothedVolume >= threshold : p.SmoothedVolume <= threshold);

                if (sustained)
                {
                    return points[i].Minutes;
                }
            }

            return null;
        }

        private static int? FindLastSustainedAbove(
            TimeOfDayProfileDto profile,
            int startMinutes,
            int endMinutes,
            double threshold,
            int sustainedBins)
        {
            var points = profile.Points
                .Where(p => p.Minutes >= startMinutes && p.Minutes <= endMinutes)
                .OrderBy(p => p.Minutes)
                .ToList();
            var runLength = 0;
            int? runEndTime = null;

            for (var i = points.Count - 1; i >= 0; i--)
            {
                if (points[i].SmoothedVolume >= threshold)
                {
                    runLength++;
                    runEndTime = points[i].Minutes;
                    if (runLength >= sustainedBins)
                    {
                        return runEndTime;
                    }
                }
                else
                {
                    runLength = 0;
                    runEndTime = null;
                }
            }

            return null;
        }

        private static int[] NormalizeBoundaries(IReadOnlyList<int> boundaries, int binSize)
        {
            var result = boundaries
                .Select(b => (int)Math.Round(b / (double)binSize) * binSize)
                .Select(b => Math.Clamp(b, binSize, 24 * 60 - binSize))
                .ToArray();

            for (var i = 1; i < result.Length; i++)
            {
                if (result[i] <= result[i - 1])
                {
                    result[i] = Math.Min(result[i - 1] + binSize, 24 * 60 - binSize);
                }
            }

            return result;
        }

        private static int InferBinSize(TimeOfDayProfileDto profile)
        {
            return profile.Points.Count > 1
                ? profile.Points[1].Minutes - profile.Points[0].Minutes
                : 15;
        }

        private static int ParseTimeOrDefault(string value, int defaultMinutes)
        {
            return TimeOnly.TryParse(value, out var time)
                ? time.Hour * 60 + time.Minute
                : defaultMinutes;
        }

        private static double Percentile(IReadOnlyList<double> values, double percentile)
        {
            if (values.Count == 0)
            {
                return 0;
            }

            var ordered = values.OrderBy(v => v).ToList();
            var position = (ordered.Count - 1) * percentile;
            var lowerIndex = (int)Math.Floor(position);
            var upperIndex = (int)Math.Ceiling(position);

            if (lowerIndex == upperIndex)
            {
                return ordered[lowerIndex];
            }

            var fraction = position - lowerIndex;
            return ordered[lowerIndex] + (ordered[upperIndex] - ordered[lowerIndex]) * fraction;
        }

        private static void AddPlan(List<Plan> schedule, string planNumber, DateTime start, DateTime end)
        {
            if (end > start)
            {
                schedule.Add(new Plan(planNumber, start, end));
            }
        }
    }
}
