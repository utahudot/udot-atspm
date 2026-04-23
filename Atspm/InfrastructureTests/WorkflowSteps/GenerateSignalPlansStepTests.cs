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
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.ATSPM.Infrastructure.Workflows;
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
        public async Task Process_GroupsByLocationAndParam_ProducesMultipleChunks()
        {
            // Arrange
            var sut = new GenerateSignalPlansStep();
            var startTime = DateTime.Now.Date;

            var inputEvents = new List<IndianaEvent>
            {
                // Location 1, Plan 1
                new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime },
                // Location 1, Plan 2
                new() { LocationIdentifier = "L1", EventParam = 2, EventCode = 131, Timestamp = startTime.AddHours(1) },
                // Location 2, Plan 1
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

            // Assert
            // Based on GroupBy(Location, Param): (L1, 1), (L1, 2), (L2, 1) = 3 distinct groups
            Assert.Equal(3, results.Count);

            // Verify one specific plan mapping
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

            // These events share a GroupBy key (Location, Param)
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
