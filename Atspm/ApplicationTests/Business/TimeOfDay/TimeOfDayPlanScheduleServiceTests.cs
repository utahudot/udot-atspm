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
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
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
                TimeOfDayDataSource.Aggregated,
                new List<TimeOfDayLocationReportData> { reportData },
                new List<DateOnly> { selectedDate },
                15);

            var schedule = result.LocationSchedules["1001"];

            Assert.True(result.HasPlanDataByLocation["1001"]);
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
        public void BuildCurrentSchedules_SeedsMidnightFromSevenDayIndianaLookback()
        {
            var selectedDate = new DateOnly(2026, 1, 8);
            var dayStart = selectedDate.ToDateTime(TimeOnly.MinValue);
            var reportData = IndianaReportData(
                IndianaPlanEvent(dayStart.AddDays(-6).AddHours(-2), 7),
                IndianaPlanEvent(dayStart.AddHours(7), 1));

            var result = new TimeOfDayPlanScheduleService().BuildCurrentSchedules(
                TimeOfDayDataSource.IndianaEvents,
                new List<TimeOfDayLocationReportData> { reportData },
                new List<DateOnly> { selectedDate },
                15);

            var schedule = result.LocationSchedules["1001"];
            Assert.True(result.HasPlanDataByLocation["1001"]);
            Assert.Equal("7", schedule[0].PlanNumber);
            Assert.Equal(dayStart, schedule[0].Start);
            Assert.Equal(dayStart.AddHours(7), schedule[0].End);
        }

        [Fact]
        public void BuildCurrentSchedules_IgnoresIndianaPlanHistoryOlderThanSevenDays()
        {
            var selectedDate = new DateOnly(2026, 1, 8);
            var dayStart = selectedDate.ToDateTime(TimeOnly.MinValue);
            var reportData = IndianaReportData(IndianaPlanEvent(dayStart.AddDays(-8), 7));

            var result = new TimeOfDayPlanScheduleService().BuildCurrentSchedules(
                TimeOfDayDataSource.IndianaEvents,
                new List<TimeOfDayLocationReportData> { reportData },
                new List<DateOnly> { selectedDate },
                15);

            Assert.False(result.HasPlanDataByLocation["1001"]);
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
            var events = new List<IndianaEvent>();
            for (var i = 0; i < selectedDates.Count; i++)
            {
                var start = selectedDates[i].ToDateTime(TimeOnly.MinValue);
                events.Add(IndianaPlanEvent(start.AddHours(-1), 7));
                events.Add(IndianaPlanEvent(start.AddHours(7), i < 2 ? (short)1 : (short)13));
                events.Add(IndianaPlanEvent(start.AddHours(9), 7));
            }

            var reportData = IndianaReportData(events.ToArray());
            var result = new TimeOfDayPlanScheduleService().BuildCurrentSchedules(
                TimeOfDayDataSource.IndianaEvents,
                new List<TimeOfDayLocationReportData> { reportData },
                selectedDates,
                15);

            var schedule = result.LocationSchedules["1001"];
            Assert.Equal("1", schedule.Single(plan => plan.Start.Hour == 7).PlanNumber);
            Assert.Equal(9, schedule.Single(plan => plan.PlanNumber == "1").End.Hour);
        }

        private static TimeOfDayLocationReportData IndianaReportData(params IndianaEvent[] events)
        {
            var reportData = new TimeOfDayLocationReportData
            {
                Location = new Location { LocationIdentifier = "1001" }
            };
            reportData.IndianaPlanEvents.AddRange(events);
            return reportData;
        }

        private static IndianaEvent IndianaPlanEvent(DateTime timestamp, short planNumber)
        {
            return new IndianaEvent
            {
                LocationIdentifier = "1001",
                EventCode = (short)IndianaEnumerations.CoordPatternChange,
                EventParam = planNumber,
                Timestamp = timestamp
            };
        }
    }
}
