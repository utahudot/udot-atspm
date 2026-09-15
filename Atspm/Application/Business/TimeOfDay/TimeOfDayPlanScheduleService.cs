#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeOfDay/TimeOfDayPlanScheduleService.cs
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
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public class TimeOfDayPlanScheduleResult
    {
        public Dictionary<string, List<Plan>> LocationSchedules { get; set; } = new();
        public Dictionary<string, bool> HasPlanDataByLocation { get; set; } = new();
        public TimeOfDayPlanComparisonDto Comparison { get; set; } = new();
    }

    public interface ITimeOfDayPlanScheduleService
    {
        TimeOfDayPlanScheduleResult BuildCurrentSchedules(
            TimeOfDayDataSource dataSource,
            IReadOnlyList<TimeOfDayLocationReportData> locationData,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes);
    }

    public class TimeOfDayPlanScheduleService : ITimeOfDayPlanScheduleService
    {
        private const int PlanLookbackDays = 7;

        private record DailyPlanSchedule(DateOnly Date, List<Plan> Plans);

        public TimeOfDayPlanScheduleResult BuildCurrentSchedules(
            TimeOfDayDataSource dataSource,
            IReadOnlyList<TimeOfDayLocationReportData> locationData,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes)
        {
            var result = new TimeOfDayPlanScheduleResult();
            var representativeDate = selectedDates.FirstOrDefault();

            foreach (var data in locationData)
            {
                var schedulesByDate = dataSource == TimeOfDayDataSource.Aggregated
                    ? GetAggregatedDailySchedules(data.SignalTimingPlans, selectedDates)
                    : GetIndianaDailySchedules(data.Location.LocationIdentifier, data.IndianaPlanEvents, selectedDates);

                var schedule = BuildRepresentativeSchedule(
                    schedulesByDate,
                    representativeDate,
                    binSizeMinutes);

                result.LocationSchedules[data.Location.LocationIdentifier] = schedule;
                result.HasPlanDataByLocation[data.Location.LocationIdentifier] = schedulesByDate.Count > 0;
            }

            result.Comparison = BuildComparison(result.LocationSchedules, locationData.Select(d => d.Location).ToList());
            return result;
        }

        private List<DailyPlanSchedule> GetIndianaDailySchedules(
            string locationIdentifier,
            IReadOnlyList<IndianaEvent> indianaPlanEvents,
            IReadOnlyList<DateOnly> selectedDates)
        {
            var schedules = new List<DailyPlanSchedule>();

            foreach (var selectedDate in selectedDates)
            {
                var start = selectedDate.ToDateTime(TimeOnly.MinValue);
                var end = start.AddDays(1);
                var events = indianaPlanEvents
                    .Where(e => e.EventCode == (short)IndianaEnumerations.CoordPatternChange)
                    .Where(e => e.Timestamp >= start.AddDays(-PlanLookbackDays) && e.Timestamp < end)
                    .OrderBy(e => e.Timestamp)
                    .ToList();

                if (events.Count == 0)
                {
                    continue;
                }

                var daily = BuildDailyPlansFromIndianaEvents(locationIdentifier, events, start, end);
                if (daily.Count > 0)
                {
                    schedules.Add(new DailyPlanSchedule(selectedDate, daily));
                }
            }

            return schedules;
        }

        private List<DailyPlanSchedule> GetAggregatedDailySchedules(
            IReadOnlyList<SignalTimingPlan> signalTimingPlans,
            IReadOnlyList<DateOnly> selectedDates)
        {
            var schedules = new List<DailyPlanSchedule>();

            foreach (var selectedDate in selectedDates)
            {
                var start = selectedDate.ToDateTime(TimeOnly.MinValue);
                var end = start.AddDays(1);
                var plans = signalTimingPlans
                    .Where(a => a.Start < end && (a.End == DateTime.MinValue || a.End > start))
                    .OrderBy(a => a.Start)
                    .ToList();

                if (plans.Count == 0)
                {
                    continue;
                }

                var daily = plans
                    .Select(a => new Plan(
                        a.PlanNumber.ToString(),
                        a.Start < start ? start : a.Start,
                        a.End == DateTime.MinValue || a.End > end ? end : a.End))
                    .Where(p => p.End > p.Start)
                    .ToList();

                schedules.Add(new DailyPlanSchedule(selectedDate, CollapsePlans(daily)));
            }

            return schedules;
        }

        private static List<Plan> BuildDailyPlansFromIndianaEvents(
            string locationIdentifier,
            IReadOnlyList<IndianaEvent> events,
            DateTime start,
            DateTime end)
        {
            var effectiveEvents = new List<IndianaEvent>();
            var priorEvent = events
                .Where(e => e.Timestamp < start)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefault();
            var eventsInRange = events
                .Where(e => e.Timestamp >= start && e.Timestamp < end)
                .OrderBy(e => e.Timestamp)
                .ToList();

            if (eventsInRange.FirstOrDefault()?.Timestamp == start)
            {
                effectiveEvents.Add(eventsInRange[0]);
                eventsInRange.RemoveAt(0);
            }
            else if (priorEvent != null)
            {
                effectiveEvents.Add(new IndianaEvent
                {
                    LocationIdentifier = locationIdentifier,
                    EventCode = (short)IndianaEnumerations.CoordPatternChange,
                    EventParam = priorEvent.EventParam,
                    Timestamp = start
                });
            }
            else
            {
                effectiveEvents.Add(new IndianaEvent
                {
                    LocationIdentifier = locationIdentifier,
                    EventCode = (short)IndianaEnumerations.CoordPatternChange,
                    EventParam = 0,
                    Timestamp = start
                });
            }

            effectiveEvents.AddRange(eventsInRange);
            effectiveEvents = effectiveEvents
                .OrderBy(e => e.Timestamp)
                .Where(e => e.Timestamp >= start && e.Timestamp < end)
                .ToList();

            var collapsed = new List<IndianaEvent>();
            foreach (var planEvent in effectiveEvents)
            {
                if (collapsed.Count == 0 || collapsed[^1].EventParam != planEvent.EventParam)
                {
                    collapsed.Add(planEvent);
                }
            }

            var plans = new List<Plan>();
            for (var i = 0; i < collapsed.Count; i++)
            {
                var planStart = collapsed[i].Timestamp < start ? start : collapsed[i].Timestamp;
                var planEnd = i + 1 < collapsed.Count ? collapsed[i + 1].Timestamp : end;
                if (planEnd > planStart)
                {
                    plans.Add(new Plan(collapsed[i].EventParam.ToString(), planStart, planEnd));
                }
            }

            return CollapsePlans(plans);
        }

        private static List<Plan> BuildRepresentativeSchedule(
            IReadOnlyList<DailyPlanSchedule> schedulesByDate,
            DateOnly representativeDate,
            int binSizeMinutes)
        {
            if (schedulesByDate.Count == 0 || representativeDate == default)
            {
                return new List<Plan>();
            }

            var binCount = 24 * 60 / binSizeMinutes;
            var representativePlans = new string[binCount];

            for (var i = 0; i < binCount; i++)
            {
                var minutes = i * binSizeMinutes;
                var plansAtBin = schedulesByDate
                    .Select(schedule =>
                    {
                        var binStart = schedule.Date.ToDateTime(TimeOnly.MinValue).AddMinutes(minutes);
                        return schedule.Plans.FirstOrDefault(p => p.Start <= binStart && p.End > binStart)?.PlanNumber ?? "0";
                    })
                    .ToList();

                representativePlans[i] = plansAtBin
                    .GroupBy(p => p)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => plansAtBin.IndexOf(g.Key))
                    .First()
                    .Key;
            }

            var representativeStart = representativeDate.ToDateTime(TimeOnly.MinValue);
            return BuildPlansFromBinSequence(representativePlans, representativeStart, binSizeMinutes);
        }

        private static List<Plan> BuildPlansFromBinSequence(
            IReadOnlyList<string> planNumbers,
            DateTime representativeStart,
            int binSizeMinutes)
        {
            var result = new List<Plan>();
            if (planNumbers.Count == 0)
            {
                return result;
            }

            var currentPlan = planNumbers[0];
            var segmentStart = representativeStart;

            for (var i = 1; i < planNumbers.Count; i++)
            {
                if (planNumbers[i] == currentPlan)
                {
                    continue;
                }

                var segmentEnd = representativeStart.AddMinutes(i * binSizeMinutes);
                result.Add(new Plan(currentPlan, segmentStart, segmentEnd));
                currentPlan = planNumbers[i];
                segmentStart = segmentEnd;
            }

            result.Add(new Plan(currentPlan, segmentStart, representativeStart.AddDays(1)));
            return CollapsePlans(result);
        }

        private static TimeOfDayPlanComparisonDto BuildComparison(
            IReadOnlyDictionary<string, List<Plan>> schedules,
            IReadOnlyList<Location> locations)
        {
            if (schedules.Count == 0 || schedules.Values.All(s => s.Count == 0))
            {
                return new TimeOfDayPlanComparisonDto
                {
                    SummaryText = "Current schedule unavailable."
                };
            }

            var orderedLocationIds = locations.Select(l => l.LocationIdentifier).ToList();
            var grouped = schedules
                .Where(kvp => kvp.Value.Count > 0)
                .GroupBy(kvp => BuildScheduleKey(kvp.Value))
                .OrderByDescending(g => g.Count())
                .ThenBy(g => orderedLocationIds.IndexOf(g.First().Key))
                .First();

            var commonSchedule = grouped.First().Value;
            var commonKey = grouped.Key;
            var exceptions = schedules
                .Where(kvp => BuildScheduleKey(kvp.Value) != commonKey)
                .Select(kvp => kvp.Key)
                .OrderBy(id => orderedLocationIds.IndexOf(id))
                .ToList();

            return new TimeOfDayPlanComparisonDto
            {
                CommonCurrentSchedule = commonSchedule,
                ExceptionLocationIdentifiers = exceptions,
                SummaryText = exceptions.Count == 0
                    ? "Current schedule is common across selected locations."
                    : $"Common current schedule found for {schedules.Count - exceptions.Count} of {schedules.Count} selected locations.",
                ExceptionsText = exceptions.Count == 0
                    ? string.Empty
                    : $"Locations with current schedule exceptions: {string.Join(", ", exceptions)}."
            };
        }

        private static string BuildScheduleKey(IReadOnlyList<Plan> schedule)
        {
            return string.Join(
                "|",
                schedule.Select(p => $"{p.PlanNumber}:{p.Start:HHmm}-{p.End:HHmm}"));
        }

        private static List<Plan> CollapsePlans(IEnumerable<Plan> plans)
        {
            var result = new List<Plan>();

            foreach (var plan in plans.OrderBy(p => p.Start))
            {
                if (plan.End <= plan.Start)
                {
                    continue;
                }

                if (result.Count > 0 &&
                    result[^1].PlanNumber == plan.PlanNumber &&
                    result[^1].End == plan.Start)
                {
                    var previous = result[^1];
                    result[^1] = new Plan(previous.PlanNumber, previous.Start, plan.End);
                }
                else
                {
                    result.Add(plan);
                }
            }

            return result;
        }
    }
}
