#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeOfDay/TimeOfDayObservationService.cs
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

using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public record TimeOfDayVolumeObservation(
        string LocationIdentifier,
        string LocationDescription,
        DateOnly LocalDate,
        int Minutes,
        string Direction,
        string Movement,
        string MovementLabel,
        double Count);

    public record TimeOfDayObservationBuildResult(
        List<TimeOfDayVolumeObservation> Observations,
        bool HasEligibleDetectors);

    public interface ITimeOfDayObservationService
    {
        TimeOfDayObservationBuildResult BuildIndianaEventObservations(
            Location location,
            string locationDescription,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes,
            IReadOnlyList<IndianaEvent> indianaEvents);

        TimeOfDayObservationBuildResult BuildAggregatedObservations(
            Location location,
            string locationDescription,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes,
            IReadOnlyList<DetectorEventCountAggregation> aggregations);
    }

    public class TimeOfDayObservationService : ITimeOfDayObservationService
    {
        private const int TurningMovementCountsMetricTypeId = 5;

        private record DateWindow(DateOnly LocalDate, DateTime Start, DateTime End);

        private record TimedDetectorEvent(Detector Detector, DateTime Timestamp);

        public TimeOfDayObservationBuildResult BuildIndianaEventObservations(
            Location location,
            string locationDescription,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes,
            IReadOnlyList<IndianaEvent> indianaEvents)
        {
            var detectorsByChannel = TimeOfDayDetectorHelper
                .GetVehicleDetectors(location)
                .Where(d => d.SupportsMetricType(TurningMovementCountsMetricTypeId))
                .GroupBy(d => (short)d.DetectorChannel)
                .ToDictionary(g => g.Key, g => g.First());

            if (detectorsByChannel.Count == 0)
            {
                return new TimeOfDayObservationBuildResult(new List<TimeOfDayVolumeObservation>(), false);
            }

            var dateWindows = BuildDateWindows(selectedDates);
            var observations = indianaEvents
                .Where(e => e.EventCode == (short)IndianaEnumerations.VehicleDetectorOn)
                .Distinct()
                .SelectMany(e => detectorsByChannel.TryGetValue(e.EventParam, out var detector)
                    ? new[]
                    {
                        new TimedDetectorEvent(
                            detector,
                            e.Timestamp
                                .AddMilliseconds(detector.GetOffset())
                                .AddSeconds(-detector.LatencyCorrection))
                    }
                    : Enumerable.Empty<TimedDetectorEvent>())
                .SelectMany(e => dateWindows
                    .Where(w => e.Timestamp >= w.Start && e.Timestamp < w.End)
                    .Select(w => CreateObservation(
                        location,
                        locationDescription,
                        w.LocalDate,
                        e.Timestamp,
                        e.Detector,
                        binSizeMinutes,
                        1)))
                .ToList();

            return new TimeOfDayObservationBuildResult(observations, true);
        }

        public TimeOfDayObservationBuildResult BuildAggregatedObservations(
            Location location,
            string locationDescription,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes,
            IReadOnlyList<DetectorEventCountAggregation> aggregations)
        {
            var detectorsById = TimeOfDayDetectorHelper
                .GetVehicleDetectors(location)
                .GroupBy(d => d.Id)
                .ToDictionary(g => g.Key, g => g.First());

            if (detectorsById.Count == 0)
            {
                return new TimeOfDayObservationBuildResult(new List<TimeOfDayVolumeObservation>(), false);
            }

            var dateWindows = BuildDateWindows(selectedDates);
            var observations = aggregations
                .Where(a => a.EventCount > 0)
                .SelectMany(a => detectorsById.TryGetValue(a.DetectorPrimaryId, out var detector)
                    ? dateWindows
                        .Where(w => a.Start >= w.Start && a.Start < w.End)
                        .Select(w => CreateObservation(
                            location,
                            locationDescription,
                            w.LocalDate,
                            a.Start,
                            detector,
                            binSizeMinutes,
                            a.EventCount))
                    : Enumerable.Empty<TimeOfDayVolumeObservation>())
                .ToList();

            return new TimeOfDayObservationBuildResult(observations, true);
        }

        private static List<DateWindow> BuildDateWindows(IReadOnlyList<DateOnly> selectedDates)
        {
            return selectedDates
                .Select(d =>
                {
                    var start = d.ToDateTime(TimeOnly.MinValue);
                    return new DateWindow(d, start, start.AddDays(1));
                })
                .ToList();
        }

        private static TimeOfDayVolumeObservation CreateObservation(
            Location location,
            string locationDescription,
            DateOnly localDate,
            DateTime timestamp,
            Detector detector,
            int binSizeMinutes,
            double count)
        {
            var minutes = timestamp.Hour * 60 + timestamp.Minute;
            var binMinutes = minutes / binSizeMinutes * binSizeMinutes;
            var direction = TimeOfDayDirectionHelper.GetDisplayName(detector.Approach.DirectionTypeId);
            var movement = TimeOfDayDirectionHelper.GetDisplayName(detector.MovementType);

            return new TimeOfDayVolumeObservation(
                location.LocationIdentifier,
                locationDescription,
                localDate,
                binMinutes,
                direction,
                movement,
                movement,
                count);
        }
    }

    internal static class TimeOfDayDetectorHelper
    {
        public static List<Detector> GetVehicleDetectors(Location location)
        {
            return location.Approaches?
                .SelectMany(a =>
                {
                    foreach (var detector in a.Detectors ?? Enumerable.Empty<Detector>())
                    {
                        detector.Approach ??= a;
                    }

                    return a.Detectors ?? Enumerable.Empty<Detector>();
                })
                .Where(d => d.LaneType == LaneTypes.V)
                .ToList() ?? new List<Detector>();
        }
    }
}
