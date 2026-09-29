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
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.TimeOfDay;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.TempExtensions;
using Xunit;

namespace Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay
{
    public class TimeOfDayPlanScheduleServiceTests
    {
        private static readonly DateOnly Date = new(2026, 4, 6);
        private static readonly DateTime Start = Date.ToDateTime(TimeOnly.MinValue);

        [Fact]
        public void BuildCurrentSchedules_UsesControllerPlanChangesAndLatestPlanBeforeMidnight()
        {
            var data = ReportData("1001");
            AddEvents(data, Date, PlanEvent(Start.AddHours(-10), 254), PlanEvent(Start.AddHours(-1), 7),
                PlanEvent(Start.AddHours(6), 1), PlanEvent(Start.AddHours(10), 7), PlanEvent(Start.AddHours(22), 254));

            var result = Build(data, Date);

            foreach (var schedule in new[] { result.LocationSchedules["1001"], result.DailySchedules["1001"].Single().Plans })
            {
                Assert.Equal(new[] { "7", "1", "7", "254" }, schedule.Select(plan => plan.PlanNumber));
                Assert.Equal(new[] { Start, Start.AddHours(6), Start.AddHours(10), Start.AddHours(22) }, schedule.Select(plan => plan.Start));
                Assert.Equal(new[] { Start.AddHours(6), Start.AddHours(10), Start.AddHours(22), Start.AddDays(1) }, schedule.Select(plan => plan.End));
            }
        }

        [Fact]
        public void BuildCurrentSchedules_SelectsMostCommonPlanAndIgnoresMissingDays()
        {
            var data = ReportData("1001");
            var dates = Enumerable.Range(0, 5).Select(Date.AddDays).ToArray();
            for (var i = 0; i < 3; i++)
            {
                var start = dates[i].ToDateTime(TimeOnly.MinValue);
                AddEvents(data, dates[i], PlanEvent(start, 7), PlanEvent(start.AddHours(7), i < 2 ? (short)1 : (short)13),
                    PlanEvent(start.AddHours(9), 7));
            }

            var result = Build(data, dates);

            var schedule = result.LocationSchedules["1001"];
            Assert.Equal(new[] { "7", "1", "7" }, schedule.Select(plan => plan.PlanNumber));
            Assert.Equal(Start.AddHours(7), schedule[1].Start);
            Assert.Equal(Start.AddHours(9), schedule[1].End);
        }

        [Fact]
        public void BuildCurrentSchedules_PreservesShortIntervalsButKeepsRepresentativeSampling()
        {
            var data = ReportData("1001");
            AddEvents(data, Date, PlanEvent(Start, 1), PlanEvent(Start.AddHours(8).AddMinutes(2), 3),
                PlanEvent(Start.AddHours(8).AddMinutes(10), 7));

            var result = Build(data, Date);

            var shortPlan = Assert.Single(result.DailySchedules["1001"].Single().Plans.Where(plan => plan.PlanNumber == "3"));
            Assert.Equal(Start.AddHours(8).AddMinutes(2), shortPlan.Start);
            Assert.Equal(Start.AddHours(8).AddMinutes(10), shortPlan.End);
            Assert.DoesNotContain(result.LocationSchedules["1001"], plan => plan.PlanNumber == "3");
        }

        [Fact]
        public void BuildCurrentSchedules_DoesNotInventPlanZeroBeforeFirstRecordedPlan()
        {
            var data = ReportData("1001");
            AddEvents(data, Date, PlanEvent(Start.AddHours(8), 7));

            var result = Build(data, Date);

            var plan = Assert.Single(result.LocationSchedules["1001"]);
            Assert.Equal("7", plan.PlanNumber);
            Assert.Equal(Start.AddHours(8), plan.Start);
            Assert.Equal(Start.AddDays(1), plan.End);
        }

        [Fact]
        public void BuildCurrentSchedules_MissingLocationsAreNotScheduleExceptions()
        {
            var data = ReportData("1001");
            AddEvents(data, Date, PlanEvent(Start, 7));
            var different = ReportData("1002");
            AddEvents(different, Date, PlanEvent(Start, 9));
            var missing = ReportData("1003");

            var result = new TimeOfDayPlanScheduleService(new PlanService())
                .BuildCurrentSchedules(new[] { data, different, missing }, new[] { Date }, 15);

            Assert.Empty(result.LocationSchedules["1003"]);
            Assert.Equal(new[] { "1002" }, result.Comparison.ExceptionLocationIdentifiers);
            Assert.Contains("1 selected locations have no plan data", result.Comparison.SummaryText);
        }

        [Fact]
        public void BuildCurrentSchedules_AllMissingPlansRemainUnavailable()
        {
            var result = Build(ReportData("1001"), Date);

            Assert.Empty(result.LocationSchedules["1001"]);
            Assert.Empty(result.Comparison.CommonCurrentSchedule);
            Assert.Empty(result.Comparison.ExceptionLocationIdentifiers);
        }

        private static TimeOfDayPlanScheduleResult Build(TimeOfDayLocationReportData data, params DateOnly[] dates) =>
            new TimeOfDayPlanScheduleService(new PlanService()).BuildCurrentSchedules(new[] { data }, dates, 15);

        private static TimeOfDayLocationReportData ReportData(string identifier) => new()
        {
            Location = new Location { LocationIdentifier = identifier }
        };

        private static void AddEvents(TimeOfDayLocationReportData data, DateOnly date, params IndianaEvent[] events)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            data.PlanEventsByDate[date] = events.GetPlanEvents(start.AddHours(-12), start.AddDays(1).AddHours(12));
        }

        private static IndianaEvent PlanEvent(DateTime timestamp, short planNumber) => new()
        {
            LocationIdentifier = "1001", Timestamp = timestamp, EventCode = 131, EventParam = planNumber
        };
    }
}