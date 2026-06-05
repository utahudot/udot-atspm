#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.InfrastructureTests.WorkflowSteps/GenerateSignalPlansStepTests.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.WorkflowSteps;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.InfrastructureTests.WorkflowSteps
{
    public class GenerateSignalPlansStepTests(ITestOutputHelper output) : IDisposable
    {
        private readonly ITestOutputHelper _output = output;

        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Cancellation")]
        public async Task GenerateSignalPlansStep_Cancellation_ClosesBlocks()
        {
            // Arrange
            var source = new CancellationTokenSource();
            var options = new ExecutionDataflowBlockOptions { CancellationToken = source.Token };
            var sut = new GenerateSignalPlansStep(options);

            // Act
            source.Cancel();

            // Assert
            // In TPL steps, cancellation usually leads to the Completion task being canceled
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await sut.Completion);
            Assert.True(sut.Completion.IsCanceled);
        }

        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Process")]
        public async Task Process_GroupsByLocation_PreservesReturningPlanNumbers()
        {
            // Arrange
            var sut = new GenerateSignalPlansStep();
            var startTime = DateTime.Now.Date;

            var inputEvents = new List<IndianaEvent>
            {
                new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime },
                new() { LocationIdentifier = "L1", EventParam = 2, EventCode = 131, Timestamp = startTime.AddHours(1) },
                new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime.AddHours(2) },
                new() { LocationIdentifier = "L2", EventParam = 1, EventCode = 131, Timestamp = startTime }
            };

            // Act
            // Post the input to the step's target block
            sut.Post(inputEvents);
            sut.Complete(); // Signal that no more data is coming

            // Receive the results from the source block
            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out var chunk))
                {
                    results.Add(chunk);
                }
            }

            Assert.Equal(2, results.Count);

            var l1Plans = results.SelectMany(c => c)
                .Where(p => p.LocationIdentifier == "L1")
                .OrderBy(p => p.Start)
                .ToList();
            Assert.Equal(new short[] { 1, 2, 1 }, l1Plans.Select(p => p.PlanNumber).ToArray());

            var l2Plan = results.SelectMany(c => c).First(p => p.LocationIdentifier == "L2");
            Assert.Equal(1, l2Plan.PlanNumber);
            Assert.Equal(startTime, l2Plan.Start);
        }

        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Process")]
        public async Task Process_FiltersOutNonPlanEvents_YieldsCorrectPlans()
        {
            // Arrange
            var sut = new GenerateSignalPlansStep();
            var startTime = DateTime.Now.Date;

            var inputEvents = new List<IndianaEvent>
            {
                new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime }, // Valid
                new() { LocationIdentifier = "L1", EventParam = 99, EventCode = 1, Timestamp = startTime.AddSeconds(1) } // Invalid code
            };

            // Act
            sut.Post(inputEvents);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                if (sut.TryReceive(out var chunk)) results.Add(chunk);
            }

            // Assert
            var flatPlans = results.SelectMany(c => c).ToList();
            Assert.Single(flatPlans);
            Assert.Equal(1, flatPlans[0].PlanNumber);
        }

        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Process")]
        public async Task Process_HandlesSequentialDuplicatesWithinGroup()
        {
            // Arrange
            var sut = new GenerateSignalPlansStep();
            var startTime = DateTime.Now.Date;

            // These events share a location and plan number.
            var inputEvents = new List<IndianaEvent>
            {
                new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime },
                new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime.AddMinutes(1) } // Sequential Duplicate
            };

            // Act
            sut.Post(inputEvents);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                if (sut.TryReceive(out var chunk)) results.Add(chunk);
            }

            // Assert
            // KeepFirstSequentialParam should reduce the 2 events into 1 SignalTimingPlan
            var flatPlans = results.SelectMany(c => c).ToList();
            Assert.Single(flatPlans);
        }

        public void Dispose()
        {
        }
    }
}
