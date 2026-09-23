#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay/TimeOfDayPlanScheduleServiceTests.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Utah.Udot.Atspm.Business.TimeOfDay;
using Utah.Udot.Atspm.Data.Models;
using Xunit;

namespace Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay
{
    public class TimeOfDayPlanScheduleServiceTests
    {
        [Fact]
        public void BuildCurrentSchedules_UsesAggregatedSignalTimingPlans()
        {
            var service = new TimeOfDayPlanScheduleService();
            var selectedDate = new DateOnly(2026, 1, 1);
            var dayStart = selectedDate.ToDateTime(TimeOnly.MinValue);
            var reportData = new TimeOfDayLocationReportData
            {
                Location = new Location { LocationIdentifier = "1001" }
            };
            reportData.SignalTimingPlans.AddRange(new[]
            {
                new SignalTimingPlan
                {
                    LocationIdentifier = "1001",
                    PlanNumber = 3,
                    Start = dayStart.AddHours(-1),
                    End = dayStart.AddHours(7)
                },
                new SignalTimingPlan
                {
                    LocationIdentifier = "1001",
                    PlanNumber = 7,
                    Start = dayStart.AddHours(7),
                    End = DateTime.MinValue
                }
            });

            var result = service.BuildCurrentSchedules(
                new List<TimeOfDayLocationReportData> { reportData },
                new List<DateOnly> { selectedDate },
                15);

            var schedule = result.LocationSchedules["1001"];

            Assert.NotEmpty(result.DailySchedules["1001"]);
            Assert.Equal(2, schedule.Count);
            Assert.Equal("3", schedule[0].PlanNumber);
            Assert.Equal(dayStart, schedule[0].Start);
            Assert.Equal(dayStart.AddHours(7), schedule[0].End);
            Assert.Equal("7", schedule[1].PlanNumber);
            Assert.Equal(dayStart.AddHours(7), schedule[1].Start);
            Assert.Equal(dayStart.AddDays(1), schedule[1].End);
            Assert.Empty(result.Comparison.ExceptionLocationIdentifiers);
        }

        [Fact]
        public void BuildCurrentSchedules_UsesOverlappingPlanRegardlessOfStartDate()
        {
            var selectedDate = new DateOnly(2026, 1, 8);
            var dayStart = selectedDate.ToDateTime(TimeOnly.MinValue);
            var reportData = ReportData(
                TimingPlan(dayStart.AddDays(-30), dayStart.AddHours(7), 7),
                TimingPlan(dayStart.AddHours(7), DateTime.MinValue, 1));

            var result = new TimeOfDayPlanScheduleService().BuildCurrentSchedules(
                new List<TimeOfDayLocationReportData> { reportData },
                new List<DateOnly> { selectedDate },
                15);

            var schedule = result.LocationSchedules["1001"];
            Assert.NotEmpty(result.DailySchedules["1001"]);
            Assert.Equal("7", schedule[0].PlanNumber);
            Assert.Equal(dayStart, schedule[0].Start);
            Assert.Equal(dayStart.AddHours(7), schedule[0].End);
        }

        [Fact]
        public void BuildCurrentSchedules_IgnoresPlansOutsideSelectedDate()
        {
            var selectedDate = new DateOnly(2026, 1, 8);
            var dayStart = selectedDate.ToDateTime(TimeOnly.MinValue);
            var reportData = ReportData(
                TimingPlan(dayStart.AddDays(-1), dayStart, 7),
                TimingPlan(dayStart.AddDays(1), DateTime.MinValue, 9));

            var result = new TimeOfDayPlanScheduleService().BuildCurrentSchedules(
                new List<TimeOfDayLocationReportData> { reportData },
                new List<DateOnly> { selectedDate },
                15);

            Assert.Empty(result.DailySchedules["1001"]);
            Assert.Empty(result.LocationSchedules["1001"]);
        }

        [Fact]
        public void BuildCurrentSchedules_SelectsMostCommonPlanForEachInterval()
        {
            var selectedDates = new List<DateOnly>
            {
                new(2026, 1, 5),
                new(2026, 1, 6),
                new(2026, 1, 7)
            };
            var plans = new List<SignalTimingPlan>();
            for (var i = 0; i < selectedDates.Count; i++)
            {
                var start = selectedDates[i].ToDateTime(TimeOnly.MinValue);
                plans.Add(TimingPlan(start, start.AddHours(7), 7));
                plans.Add(TimingPlan(start.AddHours(7), start.AddHours(9), i < 2 ? (short)1 : (short)13));
                plans.Add(TimingPlan(start.AddHours(9), start.AddDays(1), 7));
            }

            var reportData = ReportData(plans.ToArray());
            var result = new TimeOfDayPlanScheduleService().BuildCurrentSchedules(
                new List<TimeOfDayLocationReportData> { reportData },
                selectedDates,
                15);

            var schedule = result.LocationSchedules["1001"];
            Assert.Equal("1", schedule.Single(plan => plan.Start.Hour == 7).PlanNumber);
            Assert.Equal(9, schedule.Single(plan => plan.PlanNumber == "1").End.Hour);
        }

        [Fact]
        public void BuildCurrentSchedules_PreservesShortDailyIntervalsBeforeSampling()
        {
            var date = new DateOnly(2026, 3, 18);
            var start = date.ToDateTime(TimeOnly.MinValue);
            var reportData = ReportData(
                TimingPlan(start.AddDays(-1), start.AddHours(8).AddMinutes(2), 1),
                TimingPlan(start.AddHours(8).AddMinutes(2), start.AddHours(8).AddMinutes(10), 3),
                TimingPlan(start.AddHours(8).AddMinutes(10), DateTime.MinValue, 7));
            var result = new TimeOfDayPlanScheduleService().BuildCurrentSchedules(
                new[] { reportData }, new[] { date, date.AddDays(1) }, 15);

            var days = result.DailySchedules["1001"];
            Assert.Equal(2, days.Count);
            var shortPlan = Assert.Single(days[0].Plans.Where(plan => plan.PlanNumber == "3"));
            Assert.Equal(start.AddHours(8).AddMinutes(2), shortPlan.Start);
            Assert.Equal(start.AddHours(8).AddMinutes(10), shortPlan.End);
            Assert.Equal(start, days[0].Plans[0].Start);
            Assert.Equal(start.AddDays(1), days[0].Plans.Last().End);
            Assert.Equal(start.AddDays(1), Assert.Single(days[1].Plans).Start);
            Assert.Equal(start.AddDays(2), days[1].Plans[0].End);
            // The representative policy is unchanged; the exact intervals are a separate output.
            Assert.DoesNotContain(result.LocationSchedules["1001"], plan => plan.PlanNumber == "3");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BuildCurrentSchedules_EndsEachPlanAtTheNextPlanChange(bool openEnded)
        {
            var date = new DateOnly(2026, 4, 6);
            var start = date.ToDateTime(TimeOnly.MinValue);
            var reportData = ReportData(
                TimingPlan(start.AddHours(-2), openEnded ? DateTime.MinValue : start.AddHours(22), 254),
                TimingPlan(start.AddHours(6), openEnded ? DateTime.MinValue : start.AddDays(1).AddHours(6), 1),
                TimingPlan(start.AddHours(10), openEnded ? DateTime.MinValue : start.AddDays(1).AddHours(10), 7),
                TimingPlan(start.AddHours(22), DateTime.MinValue, 254));

            var result = new TimeOfDayPlanScheduleService().BuildCurrentSchedules(
                new[] { reportData }, new[] { date }, 15);

            foreach (var schedule in new[] { result.LocationSchedules["1001"], result.DailySchedules["1001"].Single().Plans })
            {
                Assert.Equal(new[] { "254", "1", "7", "254" }, schedule.Select(plan => plan.PlanNumber));
                Assert.Equal(new[] { start, start.AddHours(6), start.AddHours(10), start.AddHours(22) },
                    schedule.Select(plan => plan.Start));
                Assert.Equal(new[] { start.AddHours(6), start.AddHours(10), start.AddHours(22), start.AddDays(1) },
                    schedule.Select(plan => plan.End));
            }
        }

        [Fact]
        public void BuildCurrentSchedules_UsesLatestPlanBeforeMidnightWhenRecordsOverlap()
        {
            var date = new DateOnly(2026, 4, 6);
            var start = date.ToDateTime(TimeOnly.MinValue);
            var reportData = ReportData(
                TimingPlan(start.AddDays(-2), DateTime.MinValue, 254),
                TimingPlan(start.AddDays(-1), DateTime.MinValue, 7));

            var result = new TimeOfDayPlanScheduleService().BuildCurrentSchedules(
                new[] { reportData }, new[] { date }, 15);

            var representative = Assert.Single(result.LocationSchedules["1001"]);
            var daily = Assert.Single(result.DailySchedules["1001"].Single().Plans);
            Assert.Equal("7", representative.PlanNumber);
            Assert.Equal("7", daily.PlanNumber);
            Assert.Equal(start, daily.Start);
            Assert.Equal(start.AddDays(1), daily.End);
        }

        private static TimeOfDayLocationReportData ReportData(params SignalTimingPlan[] plans)
        {
            var reportData = new TimeOfDayLocationReportData
            {
                Location = new Location { LocationIdentifier = "1001" }
            };
            reportData.SignalTimingPlans.AddRange(plans);
            return reportData;
        }

        private static SignalTimingPlan TimingPlan(DateTime start, DateTime end, short planNumber)
        {
            return new SignalTimingPlan
            {
                LocationIdentifier = "1001",
                PlanNumber = planNumber,
                Start = start,
                End = end
            };
        }
    }
}
