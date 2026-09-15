#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeOfDay/TimeOfDayPlanProfileService.cs
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

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public interface ITimeOfDayPlanProfileService
    {
        TimeOfDayPlanProfileDto BuildPlanProfile(
            TimeOfDayProfileDto corridorProfile,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            IReadOnlyList<TimeOfDayLocationResult> locations);
    }

    public class TimeOfDayPlanProfileService : ITimeOfDayPlanProfileService
    {
        public TimeOfDayPlanProfileDto BuildPlanProfile(
            TimeOfDayProfileDto corridorProfile,
            IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
            IReadOnlyList<TimeOfDayLocationResult> locations)
        {
            var peaks = new List<TimeOfDayPeakEventDto>();
            AddPeak(peaks, "AM corridor peak", "Corridor", "AM", corridorProfile, 4 * 60, 11 * 60);
            AddPeak(peaks, "PM corridor peak", "Corridor", "PM", corridorProfile, 12 * 60, 19 * 60);

            foreach (var directionalProfile in directionalProfiles)
            {
                AddPeak(peaks, $"{directionalProfile.Direction} peak", "Primary", string.Empty, directionalProfile, 0, 24 * 60);
            }

            foreach (var location in locations)
            {
                AddLocationPeak(peaks, location, "AM", 5 * 60, 10 * 60);
                AddLocationPeak(peaks, location, "PM", 14 * 60, 19 * 60);
            }

            return new TimeOfDayPlanProfileDto
            {
                CorridorProfile = corridorProfile,
                DirectionalProfiles = directionalProfiles.ToList(),
                Peaks = peaks
            };
        }

        private static void AddPeak(
            List<TimeOfDayPeakEventDto> peaks,
            string label,
            string series,
            string period,
            TimeOfDayProfileDto profile,
            int startMinutes,
            int endMinutes)
        {
            var peak = FindPeak(profile, startMinutes, endMinutes);
            if (peak == null)
            {
                return;
            }

            peaks.Add(new TimeOfDayPeakEventDto
            {
                Label = label,
                Series = series,
                Period = period,
                TimeOfDay = peak.TimeOfDay,
                Minutes = peak.Minutes,
                Value = peak.SmoothedVolume,
                ValueUnits = profile.Units
            });
        }

        private static void AddLocationPeak(
            List<TimeOfDayPeakEventDto> peaks,
            TimeOfDayLocationResult location,
            string period,
            int startMinutes,
            int endMinutes)
        {
            var peak = FindPeak(location.Profile, startMinutes, endMinutes);
            if (peak == null || (peak.SmoothedVolume <= 0 && peak.AverageVolume <= 0))
            {
                return;
            }

            peaks.Add(new TimeOfDayPeakEventDto
            {
                Label = $"{period} peak",
                Series = "Location",
                Period = period,
                LocationIdentifier = location.LocationIdentifier,
                LocationDescription = location.LocationDescription,
                TimeOfDay = peak.TimeOfDay,
                Minutes = peak.Minutes,
                Value = peak.SmoothedVolume,
                ValueUnits = location.Profile.Units
            });
        }

        private static TimeOfDayProfilePointDto FindPeak(TimeOfDayProfileDto profile, int startMinutes, int endMinutes)
        {
            return profile.Points
                .Where(p => p.Minutes >= startMinutes && p.Minutes < endMinutes)
                .OrderByDescending(p => p.SmoothedVolume)
                .ThenBy(p => p.Minutes)
                .FirstOrDefault();
        }
    }
}
