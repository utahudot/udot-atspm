#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - %Namespace%/CalculateTimingPlansTests.cs
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
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class CalculateTimingPlansTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public CalculateTimingPlansTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(nameof(CalculateTimingPlansTests), "DurationCheck")]
        public async void CalculateTimingPlansDurationCheckTest()
        {
            var sut = new CalculateTimingPlans<TestPlan>();

            var testEvents = Enumerable.Range(1, 10).Select(s => new ControllerEventLog()
            {
                locationIdentifier = "1001",
                EventCode = (int)DataLoggerEnum.CoordPatternChange,
                EventParam = 1,
                Timestamp = DateTime.Now.AddSeconds(s)

            });

            var result = await sut.ExecuteAsync(testEvents);

            var condition = result.SelectMany(s => s).All(a => Math.Round((a.End - a.Start).TotalSeconds, 0) == 1);

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateTimingPlansTests), "LocationGrouping")]
        public async void CalculateTimingPlansLocationGroupingTest()
        {
            var sut = new CalculateTimingPlans<TestPlan>();

            var testEvents = Enumerable.Range(1, 10).Select(s => new ControllerEventLog()
            {
                locationIdentifier = s % 2 == 0 ? "1001" : "1002",
                EventCode = (int)DataLoggerEnum.CoordPatternChange,
                EventParam = 1,
                Timestamp = DateTime.Now.AddSeconds(s)
            });

            var result = await sut.ExecuteAsync(testEvents);

            var condition = result.Count() == 2 && result.All(a => a.Count() == 4);

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateTimingPlansTests), "PlanGrouping")]
        public async void CalculateTimingPlansPlanGroupingTest()
        {
            var sut = new CalculateTimingPlans<TestPlan>();

            var testEvents = Enumerable.Range(1, 10).Select(s => new ControllerEventLog()
            {
                locationIdentifier = "1001",
                EventCode = (int)DataLoggerEnum.CoordPatternChange,
                EventParam = s % 2 == 0 ? 1 : 2,
                Timestamp = DateTime.Now.AddSeconds(s)
            });

            var result = await sut.ExecuteAsync(testEvents);

            var condition = result.Count() == 1 && result.All(a => a.Count() == 8);

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateTimingPlansTests), "EventCodeFilterPass")]
        public async void CalculateTimingPlansEventCodeFilterPassTest()
        {
            var sut = new CalculateTimingPlans<TestPlan>();

            var testEvents = Enumerable.Range(1, 10).Select(s => new ControllerEventLog()
            {
                locationIdentifier = "1001",
                EventCode = s % 2 == 0 ? s : (int)DataLoggerEnum.CoordPatternChange,
                EventParam = 1,
                Timestamp = DateTime.Now.AddSeconds(s)

            });

            var result = await sut.ExecuteAsync(testEvents);

            var condition = result.Count() == 1 && result.All(a => a.Count() == 4);

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateTimingPlansTests), "OrderCheck")]
        public async void CalculateTimingPlansOrderCheckTest()
        {
            var sut = new CalculateTimingPlans<TestPlan>();

            var testEvents = Enumerable.Range(1, 10).Select(s => new ControllerEventLog()
            {
                locationIdentifier = "1001",
                EventCode = (int)DataLoggerEnum.CoordPatternChange,
                EventParam = 1,
                Timestamp = DateTime.Now.AddSeconds(s)

            }).OrderByDescending(o => o.Timestamp);

            var result = await sut.ExecuteAsync(testEvents);

            var condition = result.SelectMany(m => m).All(a => a.Start < a.End);

            Assert.True(condition);
        }

        public void Dispose()
        {
        }
    }

    public class TestPlan : Plan
    {
        public override void AssignToPlan<T>(IEnumerable<T> range)
        {
            throw new NotImplementedException();
        }
    }
}
