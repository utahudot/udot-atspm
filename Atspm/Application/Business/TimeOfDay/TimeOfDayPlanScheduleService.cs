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
using Utah.Udot.Atspm.Data.Models;

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    public class TimeOfDayPlanScheduleResult
    {
        public Dictionary<string, List<Plan>> LocationSchedules { get; set; } = new();
        public Dictionary<string, List<TimeOfDayDailyPlanScheduleDto>> DailySchedules { get; set; } = new();
        public TimeOfDayPlanComparisonDto Comparison { get; set; } = new();
    }

    public interface ITimeOfDayPlanScheduleService
    {
        TimeOfDayPlanScheduleResult BuildCurrentSchedules(
            IReadOnlyList<TimeOfDayLocationReportData> locationData,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes);
    }

    public class TimeOfDayPlanScheduleService : ITimeOfDayPlanScheduleService
    {
        private readonly PlanService planService;

        public TimeOfDayPlanScheduleService(PlanService planService)
        {
            this.planService = planService;
        }

        public TimeOfDayPlanScheduleResult BuildCurrentSchedules(
            IReadOnlyList<TimeOfDayLocationReportData> locationData,
            IReadOnlyList<DateOnly> selectedDates,
            int binSizeMinutes)
        {
            var result = new TimeOfDayPlanScheduleResult();
            var representativeDate = selectedDates.FirstOrDefault();

            foreach (var data in locationData)
            {
                var schedulesByDate = GetDailySchedules(data, selectedDates);

                var schedule = BuildRepresentativeSchedule(
                    schedulesByDate,
                    representativeDate,
                    binSizeMinutes);

                result.DailySchedules[data.Location.LocationIdentifier] = schedulesByDate;
                result.LocationSchedules[data.Location.LocationIdentifier] = schedule;
            }

            result.Comparison = BuildComparison(result.LocationSchedules, locationData.Select(d => d.Location).ToList());
            return result;
        }

        private List<TimeOfDayDailyPlanScheduleDto> GetDailySchedules(
            TimeOfDayLocationReportData data,
            IReadOnlyList<DateOnly> selectedDates)
        {
            var schedules = new List<TimeOfDayDailyPlanScheduleDto>();

            foreach (var selectedDate in selectedDates)
            {
                var start = selectedDate.ToDateTime(TimeOnly.MinValue);
                var end = start.AddDays(1);
                if (!data.PlanEventsByDate.TryGetValue(selectedDate, out var planEvents) || planEvents.Count == 0)
                {
                    continue;
                }

                var daily = planService.GetBasicPlans(start, end, data.Location.LocationIdentifier, planEvents)
                    .Where(p => p.End > p.Start && p.PlanNumber != "0")
                    .ToList();

                if (daily.Count > 0)
                {
                    schedules.Add(new TimeOfDayDailyPlanScheduleDto { Date = selectedDate, Plans = CollapsePlans(daily) });
                }
            }

            return schedules;
        }

        private static List<Plan> BuildRepresentativeSchedule(
            IReadOnlyList<TimeOfDayDailyPlanScheduleDto> schedulesByDate,
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
                        return schedule.Plans.FirstOrDefault(p => p.Start <= binStart && p.End > binStart)?.PlanNumber;
                    })
                    .Where(plan => plan != null)
                    .ToList();

                representativePlans[i] = plansAtBin
                    .GroupBy(p => p)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => plansAtBin.IndexOf(g.Key))
                    .FirstOrDefault()
                    ?.Key;
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
                if (currentPlan != null)
                {
                    result.Add(new Plan(currentPlan, segmentStart, segmentEnd));
                }
                currentPlan = planNumbers[i];
                segmentStart = segmentEnd;
            }

            if (currentPlan != null)
            {
                result.Add(new Plan(currentPlan, segmentStart, representativeStart.AddDays(1)));
            }
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
                .Where(kvp => kvp.Value.Count > 0 && BuildScheduleKey(kvp.Value) != commonKey)
                .Select(kvp => kvp.Key)
                .OrderBy(id => orderedLocationIds.IndexOf(id))
                .ToList();

            var availableCount = schedules.Count(schedule => schedule.Value.Count > 0);
            return new TimeOfDayPlanComparisonDto
            {
                CommonCurrentSchedule = commonSchedule,
                ExceptionLocationIdentifiers = exceptions,
                SummaryText = exceptions.Count == 0 && availableCount == schedules.Count
                    ? "Current schedule is common across selected locations."
                    : $"Common current schedule found for {availableCount - exceptions.Count} of {availableCount} locations with plan data; {schedules.Count - availableCount} selected locations have no plan data.",
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
