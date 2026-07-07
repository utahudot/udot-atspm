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
    public class GenerateSignalPlansStepTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public GenerateSignalPlansStepTests(ITestOutputHelper output)
        {
            _output = output;
        }

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

        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Process")]
        public async Task Process_GroupsByLocationAndParam_ProducesMultipleChunks()
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

            Assert.Equal(3, results.Count);

            var l2Plan = results.SelectMany(c => c).First(p => p.LocationIdentifier == "L2");
            Assert.Equal(1, l2Plan.PlanNumber);
            Assert.Equal(startTime, l2Plan.Start);
        }

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

            sut.Post(inputEvents);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                if (sut.TryReceive(out var chunk)) results.Add(chunk);
            }

            var flatPlans = results.SelectMany(c => c).ToList();
            Assert.Single(flatPlans);
            Assert.Equal(1, flatPlans[0].PlanNumber);
        }

        [Fact]
        [Trait(nameof(GenerateSignalPlansStep), "Process")]
        public async Task Process_HandlesSequentialDuplicatesWithinGroup()
        {
            var sut = new GenerateSignalPlansStep();
            var startTime = DateTime.Now.Date;

            var inputEvents = new List<IndianaEvent>
        {
            new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime },
            new() { LocationIdentifier = "L1", EventParam = 1, EventCode = 131, Timestamp = startTime.AddMinutes(1) }
        };

            sut.Post(inputEvents);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                if (sut.TryReceive(out var chunk)) results.Add(chunk);
            }

            var flatPlans = results.SelectMany(c => c).ToList();
            Assert.Single(flatPlans);
        }

        // ==========================================
        // TEST 5: THE BACKPRESSURE DRAINAGE VERIFIER
        // ==========================================
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

            // FIX: We must actively drain the output buffer loop context, otherwise the 
            // underlying block will suspend itself on backpressure limits and timeout
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

        public void Dispose()
        {
        }
    }
}
