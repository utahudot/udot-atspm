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
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.WorkflowSteps;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.InfrastructureTests.WorkflowSteps
{
    /// <summary>
    /// Unit tests for <see cref="GenerateSignalPlansStep"/>.
    /// </summary>
    public class GenerateSignalPlansStepTests : WorkflowStepTestBase<GenerateSignalPlansStep, GenerateSignalPlansTestData, Location, IEnumerable<IndianaEvent>, IEnumerable<SignalTimingPlan>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateSignalPlansStepTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper.</param>
        /// <param name="testLocationFixture">The test location fixture.</param>
        public GenerateSignalPlansStepTests(ITestOutputHelper output, TestLocationFixture testLocationFixture)
            : base(output, testLocationFixture)
        {
        }

        /// <inheritdoc/>
        protected override IEnumerable<IndianaEvent> DefaultTestInput => new List<IndianaEvent>();

        /// <inheritdoc/>
        protected override GenerateSignalPlansStep CreateStep(Location config, IEnumerable<IndianaEvent> input, IEnumerable<SignalTimingPlan> expected)
        {
            return new GenerateSignalPlansStep();
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<SignalTimingPlan>> ExecuteStepAsync(
            GenerateSignalPlansStep step,
            Location config,
            IEnumerable<IndianaEvent> input,
            CancellationToken cancelToken = default)
        {
            if (cancelToken.IsCancellationRequested)
            {
                await Task.FromCanceled(cancelToken);
            }

            step.Post(input);
            step.Complete();

            var results = new List<SignalTimingPlan>();
            while (await step.OutputAvailableAsync(cancelToken))
            {
                while (step.TryReceive(out var chunk))
                {
                    results.AddRange(chunk);
                }
            }

            await step.Completion;
            return results;
        }

        /// <inheritdoc/>
        protected override void AssertOutputs(IEnumerable<SignalTimingPlan> actual, IEnumerable<SignalTimingPlan> expected)
        {
            var actualList = actual.OrderBy(p => p.Start).ToList();
            var expectedList = expected.OrderBy(p => p.Start).ToList();

            Assert.Equal(expectedList.Count, actualList.Count);
            for (var i = 0; i < expectedList.Count; i++)
            {
                Assert.Equal(expectedList[i].LocationIdentifier, actualList[i].LocationIdentifier);
                Assert.Equal(expectedList[i].PlanNumber, actualList[i].PlanNumber);
                Assert.Equal(expectedList[i].Start, actualList[i].Start);
                Assert.Equal(expectedList[i].End, actualList[i].End);
            }
        }

        /// <summary>
        /// Verifies that cancelling the token closes the dataflow block.
        /// </summary>
        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Cancellation")]
        public async Task GenerateSignalPlansStep_Cancellation_ClosesBlocks()
        {
            var source = new CancellationTokenSource();
            var options = new ExecutionDataflowBlockOptions { CancellationToken = source.Token };
            var sut = new GenerateSignalPlansStep(options);

            source.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await sut.Completion);
            Assert.True(sut.Completion.IsCanceled);
        }

        /// <summary>
        /// Verifies that events are grouped by location into a single ordered timeline chunk per location.
        /// </summary>
        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Process")]
        public async Task Process_GroupsByLocation_ProducesChunkPerLocation()
        {
            var sut = new GenerateSignalPlansStep();
            var startTime = DateTime.Now.Date;

            var inputEvents = new List<IndianaEvent>
            {
                new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime },
                new() { LocationIdentifier = "L1", EventParam = 2, EventCode = 131, Timestamp = startTime.AddHours(1) },
                new() { LocationIdentifier = "L2", EventParam = 1, EventCode = 131, Timestamp = startTime }
            };

            sut.Post(inputEvents);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out var chunk))
                {
                    results.Add(chunk);
                }
            }

            Assert.Equal(2, results.Count);

            var l1Plans = results.First(c => c.All(p => p.LocationIdentifier == "L1")).ToList();
            Assert.Equal(2, l1Plans.Count);
            Assert.Equal(1, l1Plans[0].PlanNumber);
            Assert.Equal(2, l1Plans[1].PlanNumber);

            var l2Plan = results.SelectMany(c => c).First(p => p.LocationIdentifier == "L2");
            Assert.Equal(1, l2Plan.PlanNumber);
            Assert.Equal(startTime, l2Plan.Start);
        }

        /// <summary>
        /// Verifies that all distinct timestamp plan events within the same location are preserved in chronological order.
        /// </summary>
        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Process")]
        public async Task Process_PreservesDistinctTimestampPlansWithinSameLocation()
        {
            var sut = new GenerateSignalPlansStep();
            var date = new DateTime(2026, 7, 14);

            var inputEvents = new List<IndianaEvent>
            {
                new() { LocationIdentifier = "7115", EventParam = 1, EventCode = 131, Timestamp = date.AddHours(6) },
                new() { LocationIdentifier = "7115", EventParam = 7, EventCode = 131, Timestamp = date.AddHours(9).AddMinutes(15) },
                new() { LocationIdentifier = "7115", EventParam = 7, EventCode = 131, Timestamp = date.AddHours(9).AddMinutes(15).AddMilliseconds(100) },
                new() { LocationIdentifier = "7115", EventParam = 13, EventCode = 131, Timestamp = date.AddHours(15) },
                new() { LocationIdentifier = "7115", EventParam = 14, EventCode = 131, Timestamp = date.AddHours(16).AddMinutes(40) },
                new() { LocationIdentifier = "7115", EventParam = 13, EventCode = 131, Timestamp = date.AddHours(17).AddMinutes(50) },
                new() { LocationIdentifier = "7115", EventParam = 7, EventCode = 131, Timestamp = date.AddHours(19) },
                new() { LocationIdentifier = "7115", EventParam = 7, EventCode = 131, Timestamp = date.AddHours(19).AddMinutes(22).AddSeconds(5.4) }
            };

            var plans = (await ExecuteStepAsync(sut, TestLocation, inputEvents)).ToList();
            Assert.Equal(8, plans.Count);
            Assert.Equal(new short[] { 1, 7, 7, 13, 14, 13, 7, 7 }, plans.Select(p => p.PlanNumber).ToArray());
        }

        /// <summary>
        /// Verifies that non-plan events (EventCode != 131) are excluded.
        /// </summary>
        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Process")]
        public async Task Process_FiltersOutNonPlanEvents_YieldsCorrectPlans()
        {
            var sut = new GenerateSignalPlansStep();
            var startTime = DateTime.Now.Date;

            var inputEvents = new List<IndianaEvent>
            {
                new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime },
                new() { LocationIdentifier = "L1", EventParam = 99, EventCode = 1, Timestamp = startTime.AddSeconds(1) }
            };

            var flatPlans = (await ExecuteStepAsync(sut, TestLocation, inputEvents)).ToList();
            Assert.Single(flatPlans);
            Assert.Equal(1, flatPlans[0].PlanNumber);
        }

        /// <summary>
        /// Verifies that exact duplicate events are collapsed via IndianaEvent equality comparison.
        /// </summary>
        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Process")]
        public async Task Process_HandlesExactDuplicatesWithinGroup()
        {
            var sut = new GenerateSignalPlansStep();
            var startTime = DateTime.Now.Date;

            var inputEvents = new List<IndianaEvent>
            {
                new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime },
                new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime }
            };

            var flatPlans = (await ExecuteStepAsync(sut, TestLocation, inputEvents)).ToList();
            Assert.Single(flatPlans);
        }

        /// <summary>
        /// Verifies that corrupted payloads do not deadlock stream completion.
        /// </summary>
        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Faults")]
        public async Task Process_WithCorruptedDataPayload_VerifiesSwallowedStreamCompletion()
        {
            var sut = new GenerateSignalPlansStep();

            var corruptedEvents = new List<IndianaEvent>
            {
                new() { LocationIdentifier = null, EventParam = 1, EventCode = 131, Timestamp = DateTime.MinValue }
            };

            sut.Post(corruptedEvents);
            sut.Complete();

            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out _)) { }
            }

            try
            {
                await sut.Completion.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                Assert.Fail("The step deadlocked inside its processing lifecycle even when fully drained.");
            }

            Assert.True(sut.Completion.IsCompletedSuccessfully);
        }
    }
}
