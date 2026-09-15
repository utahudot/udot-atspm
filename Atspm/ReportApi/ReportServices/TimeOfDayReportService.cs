#region license
// Copyright 2026 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/TimeOfDayReportService.cs
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

using Utah.Udot.Atspm.Business.TimeOfDay;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    public class TimeOfDayReportService : ReportServiceBase<TimeOfDayOptions, TimeOfDayResult>
    {
        private const int PlanLookbackDays = 7;
        private const int DetectorPaddingHours = 1;

        private record LoadWindow(DateTime Start, DateTime End);

        private readonly ILocationRepository locationRepository;
        private readonly IIndianaEventLogRepository eventLogRepository;
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly ISignalTimingPlanRepository signalTimingPlanRepository;
        private readonly TimeOfDayService timeOfDayService;

        public TimeOfDayReportService(
            ILocationRepository locationRepository,
            IIndianaEventLogRepository eventLogRepository,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            ISignalTimingPlanRepository signalTimingPlanRepository,
            TimeOfDayService timeOfDayService)
        {
            this.locationRepository = locationRepository;
            this.eventLogRepository = eventLogRepository;
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            this.signalTimingPlanRepository = signalTimingPlanRepository;
            this.timeOfDayService = timeOfDayService;
        }

        public override Task<TimeOfDayResult> ExecuteAsync(
            TimeOfDayOptions parameter,
            IProgress<int> progress = null,
            CancellationToken cancelToken = default)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            NormalizeForExecution(parameter);
            Validate(parameter);

            var warnings = new List<TimeOfDayWarningDto>();
            var locationIdentifiers = parameter.LocationIdentifiers
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var selectedDates = parameter.SelectedDates
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            if (selectedDates.Count != parameter.SelectedDates.Count)
            {
                warnings.Add(new TimeOfDayWarningDto
                {
                    Code = "DuplicateSelectedDates",
                    Message = "Duplicate selected dates were normalized to distinct local calendar dates."
                });
            }

            var firstDate = selectedDates[0].ToDateTime(TimeOnly.MinValue);
            var locations = LoadLocations(locationIdentifiers, firstDate);

            var locationData = locations
                .Select(location => LoadLocationData(parameter, location, selectedDates, warnings))
                .ToList();

            return Task.FromResult(timeOfDayService.GetChartData(
                parameter,
                locationIdentifiers,
                selectedDates,
                locationData,
                warnings));
        }

        private static void Validate(TimeOfDayOptions parameter)
        {
            if (parameter.LocationIdentifiers == null || parameter.LocationIdentifiers.All(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("At least one location identifier is required.");
            }

            if (parameter.SelectedDates == null || parameter.SelectedDates.Count == 0)
            {
                throw new ArgumentException("At least one selected date is required.");
            }

            if (parameter.LaneCapacityVehiclesPerHour <= 0)
            {
                throw new ArgumentException("Lane capacity must be greater than zero.");
            }

            if (parameter.ApproachVolumeAssumedLanes <= 0)
            {
                throw new ArgumentException("Approach volume assumed lanes must be greater than zero.");
            }

            if (parameter.EntrySustainedBins <= 0 || parameter.FreeSustainedBins <= 0)
            {
                throw new ArgumentException("Sustained bin counts must be greater than zero.");
            }

            if (!TimeOnly.TryParse(parameter.FreeFallbackTime, out _) ||
                !TimeOnly.TryParse(parameter.MaxAmEndTime, out _) ||
                !TimeOnly.TryParse(parameter.MaxPmEndTime, out _))
            {
                throw new ArgumentException("Time Of Day threshold times must use a valid time value.");
            }
        }

        internal static void NormalizeForExecution(TimeOfDayOptions parameter)
        {
            parameter.BinSizeMinutes = TimeOfDayOptions.FixedBinSizeMinutes;
            parameter.LocationIdentifiers ??= new();
            parameter.SelectedDates ??= new();
            parameter.AllDayPrimaryDirections ??= new();
            parameter.AmPrimaryDirections ??= new();
            parameter.PmPrimaryDirections ??= new();
            parameter.DirectionLaneCounts ??= new();
        }

        private IReadOnlyList<Location> LoadLocations(IReadOnlyList<string> locationIdentifiers, DateTime firstDate)
        {
            var locations = new List<Location>();
            foreach (var locationIdentifier in locationIdentifiers)
            {
                var location = locationRepository.GetLatestVersionOfLocation(locationIdentifier, firstDate);
                if (location == null)
                {
                    throw new NullReferenceException($"Location {locationIdentifier} not found");
                }

                locations.Add(location);
            }

            return locations;
        }

        private TimeOfDayLocationReportData LoadLocationData(
            TimeOfDayOptions options,
            Location location,
            IReadOnlyList<DateOnly> selectedDates,
            List<TimeOfDayWarningDto> warnings)
        {
            var data = new TimeOfDayLocationReportData
            {
                Location = location,
                LocationDescription = BuildLocationDescription(location)
            };

            if (options.DataSource == TimeOfDayDataSource.Aggregated)
            {
                foreach (var selectedDate in selectedDates)
                {
                    var start = selectedDate.ToDateTime(TimeOnly.MinValue);
                    var end = start.AddDays(1);

                    data.DetectorEventCountAggregations.AddRange(
                        detectorEventCountAggregationRepository.GetAggregationsBetweenDates(location.LocationIdentifier, start, end));
                    data.SignalTimingPlans.AddRange(
                        signalTimingPlanRepository.GetList()
                            .Where(p => p.LocationIdentifier == location.LocationIdentifier
                                && p.Start < end
                                && (p.End == DateTime.MinValue || p.End > start))
                            .ToList());
                }
            }
            else
            {
                LoadIndianaEvents(location.LocationIdentifier, selectedDates, data);
            }

            if (data.SignalTimingPlans.Count > 0)
            {
                var distinctPlans = data.SignalTimingPlans
                    .DistinctBy(p => new { p.LocationIdentifier, p.PlanNumber, p.Start })
                    .ToList();

                data.SignalTimingPlans.Clear();
                data.SignalTimingPlans.AddRange(distinctPlans);
            }

            return data;
        }

        private void LoadIndianaEvents(
            string locationIdentifier,
            IReadOnlyList<DateOnly> selectedDates,
            TimeOfDayLocationReportData data)
        {
            var detectorWindows = selectedDates
                .Select(selectedDate =>
                {
                    var start = selectedDate.ToDateTime(TimeOnly.MinValue);
                    return new LoadWindow(
                        start.AddHours(-DetectorPaddingHours),
                        start.AddDays(1).AddHours(DetectorPaddingHours));
                })
                .ToList();
            var planWindows = selectedDates
                .Select(selectedDate =>
                {
                    var start = selectedDate.ToDateTime(TimeOnly.MinValue);
                    return new LoadWindow(start.AddDays(-PlanLookbackDays), start.AddDays(1));
                })
                .ToList();
            var queryWindows = MergeWindows(detectorWindows.Concat(planWindows));

            foreach (var queryWindow in queryWindows)
            {
                var controllerEventLogs = eventLogRepository
                    .GetEventsBetweenDates(locationIdentifier, queryWindow.Start, queryWindow.End);

                data.IndianaEvents.AddRange(controllerEventLogs
                    .Where(e => e.EventCode == (short)IndianaEnumerations.VehicleDetectorOn)
                    .Where(e => IsWithinAnyWindow(e.Timestamp, detectorWindows)));
                data.IndianaPlanEvents.AddRange(controllerEventLogs
                    .Where(e => e.EventCode == (short)IndianaEnumerations.CoordPatternChange)
                    .Where(e => IsWithinAnyWindow(e.Timestamp, planWindows)));
            }

            ReplaceWithDistinctChronologicalEvents(data.IndianaEvents);
            ReplaceWithDistinctChronologicalEvents(data.IndianaPlanEvents);
        }

        private static List<LoadWindow> MergeWindows(IEnumerable<LoadWindow> windows)
        {
            var orderedWindows = windows
                .Where(window => window.End > window.Start)
                .OrderBy(window => window.Start)
                .ThenBy(window => window.End)
                .ToList();
            var mergedWindows = new List<LoadWindow>();

            foreach (var window in orderedWindows)
            {
                if (mergedWindows.Count == 0 || window.Start > mergedWindows[^1].End)
                {
                    mergedWindows.Add(window);
                    continue;
                }

                var current = mergedWindows[^1];
                mergedWindows[^1] = new LoadWindow(current.Start, current.End > window.End ? current.End : window.End);
            }

            return mergedWindows;
        }

        private static bool IsWithinAnyWindow(DateTime timestamp, IReadOnlyList<LoadWindow> windows)
        {
            return windows.Any(window => timestamp >= window.Start && timestamp < window.End);
        }

        private static void ReplaceWithDistinctChronologicalEvents(List<IndianaEvent> events)
        {
            var distinctEvents = events
                .Distinct()
                .OrderBy(e => e.Timestamp)
                .ThenBy(e => e.EventCode)
                .ThenBy(e => e.EventParam)
                .ToList();

            events.Clear();
            events.AddRange(distinctEvents);
        }

        private static string BuildLocationDescription(Location location)
        {
            return $"#{location.LocationIdentifier} - {location.PrimaryName} & {location.SecondaryName}";
        }
    }
}
