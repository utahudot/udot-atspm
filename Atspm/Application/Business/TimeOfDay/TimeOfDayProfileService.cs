#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeOfDay/TimeOfDayProfileService.cs
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

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public interface ITimeOfDayProfileService
    {
        TimeOfDayProfileDto BuildProfile(
            string label,
            string direction,
            string movement,
            string movementLabel,
            IReadOnlyList<TimeOfDayVolumeObservation> observations,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes);

        TimeOfDayProfileDto SumProfiles(
            string label,
            IReadOnlyList<TimeOfDayProfileDto> profiles,
            string direction = "",
            string movement = "",
            string movementLabel = "");

        TimeOfDayProfileDto MedianProfiles(
            string label,
            IReadOnlyList<TimeOfDayProfileDto> profiles,
            string direction = "",
            string movement = "",
            string movementLabel = "");
    }

    public class TimeOfDayProfileService : ITimeOfDayProfileService
    {
        public TimeOfDayProfileDto BuildProfile(
            string label,
            string direction,
            string movement,
            string movementLabel,
            IReadOnlyList<TimeOfDayVolumeObservation> observations,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes)
        {
            var binCount = 24 * 60 / binSizeMinutes;
            var volumeMultiplier = 60d / binSizeMinutes;
            var daysWithData = observations
                .Select(o => o.LocalDate)
                .Distinct()
                .Count();
            var divisor = Math.Max(daysWithData, observations.Count > 0 ? 1 : selectedDates.Count);

            var countByMinute = observations
                .GroupBy(o => o.Minutes)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(o => o.Count) / Math.Max(divisor, 1) * volumeMultiplier);

            var participatingLocationsByMinute = observations
                .GroupBy(o => o.Minutes)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(o => o.LocationIdentifier).Distinct().Count());

            var rawPoints = Enumerable
                .Range(0, binCount)
                .Select(i =>
                {
                    var minutes = i * binSizeMinutes;
                    return new TimeOfDayProfilePointDto
                    {
                        TimeOfDay = FormatTime(minutes),
                        Minutes = minutes,
                        AverageVolume = Round(countByMinute.GetValueOrDefault(minutes)),
                        ParticipatingLocations = participatingLocationsByMinute.TryGetValue(minutes, out var participating)
                            ? participating
                            : null
                    };
                })
                .ToList();

            for (var i = 0; i < rawPoints.Count; i++)
            {
                var values = new List<double> { rawPoints[i].AverageVolume };
                if (i > 0)
                {
                    values.Add(rawPoints[i - 1].AverageVolume);
                }

                if (i + 1 < rawPoints.Count)
                {
                    values.Add(rawPoints[i + 1].AverageVolume);
                }

                rawPoints[i].SmoothedVolume = Round(values.Average());
                rawPoints[i].RollingHourVph = i >= 3
                    ? Round(rawPoints.Skip(i - 3).Take(4).Average(p => p.AverageVolume))
                    : null;
                rawPoints[i].Delta = i == 0
                    ? 0
                    : Round(rawPoints[i].SmoothedVolume - rawPoints[i - 1].SmoothedVolume);
            }

            return new TimeOfDayProfileDto
            {
                Label = label,
                Direction = direction,
                Movement = movement,
                MovementLabel = movementLabel,
                Points = rawPoints
            };
        }

        public TimeOfDayProfileDto SumProfiles(
            string label,
            IReadOnlyList<TimeOfDayProfileDto> profiles,
            string direction = "",
            string movement = "",
            string movementLabel = "")
        {
            if (profiles.Count == 0)
            {
                return new TimeOfDayProfileDto
                {
                    Label = label,
                    Direction = direction,
                    Movement = movement,
                    MovementLabel = movementLabel
                };
            }

            var pointCount = profiles.Max(p => p.Points.Count);
            var points = new List<TimeOfDayProfilePointDto>();

            for (var i = 0; i < pointCount; i++)
            {
                var template = profiles.Select(p => p.Points.ElementAtOrDefault(i)).FirstOrDefault(p => p != null);
                if (template == null)
                {
                    continue;
                }

                var average = profiles.Sum(p => p.Points.ElementAtOrDefault(i)?.AverageVolume ?? 0);
                var smoothed = profiles.Sum(p => p.Points.ElementAtOrDefault(i)?.SmoothedVolume ?? 0);
                var rollingValues = profiles
                    .Select(p => p.Points.ElementAtOrDefault(i)?.RollingHourVph)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .ToList();

                points.Add(new TimeOfDayProfilePointDto
                {
                    TimeOfDay = template.TimeOfDay,
                    Minutes = template.Minutes,
                    AverageVolume = Round(average),
                    SmoothedVolume = Round(smoothed),
                    RollingHourVph = rollingValues.Count == profiles.Count ? Round(rollingValues.Sum()) : null,
                    ParticipatingLocations = profiles
                        .Select(p => p.Points.ElementAtOrDefault(i)?.ParticipatingLocations)
                        .Where(v => v.HasValue)
                        .Sum(v => v!.Value)
                });
            }

            for (var i = 0; i < points.Count; i++)
            {
                points[i].Delta = i == 0 ? 0 : Round(points[i].SmoothedVolume - points[i - 1].SmoothedVolume);
            }

            return new TimeOfDayProfileDto
            {
                Label = label,
                Direction = direction,
                Movement = movement,
                MovementLabel = movementLabel,
                Points = points
            };
        }

        public TimeOfDayProfileDto MedianProfiles(
            string label,
            IReadOnlyList<TimeOfDayProfileDto> profiles,
            string direction = "",
            string movement = "",
            string movementLabel = "")
        {
            if (profiles.Count == 0)
            {
                return new TimeOfDayProfileDto
                {
                    Label = label,
                    Direction = direction,
                    Movement = movement,
                    MovementLabel = movementLabel
                };
            }

            var profilePointsByMinute = profiles
                .SelectMany(p => p.Points)
                .GroupBy(p => p.Minutes)
                .OrderBy(g => g.Key)
                .ToList();
            var points = new List<TimeOfDayProfilePointDto>();

            foreach (var group in profilePointsByMinute)
            {
                var groupPoints = group.ToList();
                var template = groupPoints[0];
                var rollingValues = groupPoints
                    .Select(p => p.RollingHourVph)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .ToList();

                points.Add(new TimeOfDayProfilePointDto
                {
                    TimeOfDay = template.TimeOfDay,
                    Minutes = template.Minutes,
                    AverageVolume = Round(Median(groupPoints.Select(p => p.AverageVolume).ToList())),
                    SmoothedVolume = Round(Median(groupPoints.Select(p => p.SmoothedVolume).ToList())),
                    RollingHourVph = rollingValues.Count == groupPoints.Count
                        ? Round(Median(rollingValues))
                        : null,
                    ParticipatingLocations = groupPoints.Count
                });
            }

            for (var i = 0; i < points.Count; i++)
            {
                points[i].Delta = i == 0 ? 0 : Round(points[i].SmoothedVolume - points[i - 1].SmoothedVolume);
            }

            return new TimeOfDayProfileDto
            {
                Label = label,
                Direction = direction,
                Movement = movement,
                MovementLabel = movementLabel,
                Points = points
            };
        }

        internal static string FormatTime(int minutes)
        {
            var normalized = ((minutes % (24 * 60)) + (24 * 60)) % (24 * 60);
            return $"{normalized / 60:00}:{normalized % 60:00}";
        }

        internal static double Round(double value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        internal static double Median(IReadOnlyList<double> values)
        {
            var ordered = values.OrderBy(v => v).ToList();
            var midpoint = ordered.Count / 2;

            return ordered.Count % 2 == 0
                ? (ordered[midpoint - 1] + ordered[midpoint]) / 2d
                : ordered[midpoint];
        }
    }
}
