#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/MergeExistingSignalPlansStepTests.cs
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
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.ATSPM.Infrastructure.Workflows;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.InfrastructureTests.WorkflowSteps
{
    public class ReconcileSignalPlansStepTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;

        [Fact]
        [Trait(nameof(ReconcileSignalPlansStep), "Process")]
        public async Task Process_SetsEndTime_BasedOnNextPlanStart()
        {
            // Arrange
            var sut = new ReconcileSignalPlansStep(new ExecutionDataflowBlockOptions());
            var startTime = DateTime.Now.Date;
            var loc = "1001";

            var input = new List<SignalTimingPlan>
            {
                new() { LocationIdentifier = loc, PlanNumber = 1, Start = startTime },
                new() { LocationIdentifier = loc, PlanNumber = 1, Start = startTime.AddHours(2) },
                new() { LocationIdentifier = loc, PlanNumber = 1, Start = startTime.AddHours(5) }
            };

            // Act
            sut.Post(input);
            sut.Complete();

            var results = new List<SignalTimingPlan>();
            while (await sut.OutputAvailableAsync())
            {
                // FIX: Receive the chunk (IEnumerable) then flatten it into our results list
                while (sut.TryReceive(out IEnumerable<SignalTimingPlan> chunk))
                {
                    results.AddRange(chunk);
                }
            }

            // Assert
            var ordered = results.OrderBy(p => p.Start).ToList();
            Assert.Equal(3, ordered.Count);

            // Core Logic: Current End should equal Next Start
            Assert.Equal(ordered[1].Start, ordered[0].End);
            Assert.Equal(ordered[2].Start, ordered[1].End);

            // The final plan in the sequence must have MinValue as End
            Assert.Equal(DateTime.MinValue, ordered[2].End);
        }

        [Fact]
        [Trait(nameof(ReconcileSignalPlansStep), "Process")]
        public async Task Process_OrdersPlansByStart_BeforeReconciling()
        {
            // Arrange
            var sut = new ReconcileSignalPlansStep(new ExecutionDataflowBlockOptions());
            var startTime = DateTime.Now.Date;

            var input = new List<SignalTimingPlan>
            {
                new() { LocationIdentifier = "L1", PlanNumber = 1, Start = startTime.AddHours(10) },
                new() { LocationIdentifier = "L1", PlanNumber = 1, Start = startTime }
            };

            // Act
            sut.Post(input);
            sut.Complete();

            var results = new List<SignalTimingPlan>();
            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out IEnumerable<SignalTimingPlan> chunk))
                {
                    results.AddRange(chunk);
                }
            }

            // Assert
            var sorted = results.OrderBy(p => p.Start).ToList();
            Assert.Equal(sorted[1].Start, sorted[0].End);
        }

        [Fact]
        [Trait(nameof(ReconcileSignalPlansStep), "Process")]
        public async Task Process_IsolatesTimelines_ByLocationAndPlan()
        {
            // Arrange
            var sut = new ReconcileSignalPlansStep(new ExecutionDataflowBlockOptions());
            var startTime = DateTime.Now.Date;

            var input = new List<SignalTimingPlan>
            {
                new() { LocationIdentifier = "L1", PlanNumber = 1, Start = startTime },
                new() { LocationIdentifier = "L2", PlanNumber = 1, Start = startTime.AddHours(5) }
            };

            // Act
            sut.Post(input);
            sut.Complete();

            var results = new List<SignalTimingPlan>();
            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out IEnumerable<SignalTimingPlan> chunk))
                {
                    results.AddRange(chunk);
                }
            }

            // Assert
            // Since they belong to different groups, they should NOT reconcile against each other
            Assert.All(results, plan => Assert.Equal(DateTime.MinValue, plan.End));
        }

        [Fact]
        [Trait(nameof(ReconcileSignalPlansStep), "Process")]
        public async Task Process_HandlesEmptyInput_Gracefully()
        {
            // Arrange
            var sut = new ReconcileSignalPlansStep(new ExecutionDataflowBlockOptions());

            // Act
            sut.Post(Enumerable.Empty<SignalTimingPlan>());
            sut.Complete();

            // Assert
            var resultsExist = await sut.OutputAvailableAsync();
            Assert.False(resultsExist);
        }
    }
}
