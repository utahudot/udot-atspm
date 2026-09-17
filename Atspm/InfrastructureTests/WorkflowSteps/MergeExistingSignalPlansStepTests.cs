#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.InfrastructureTests.WorkflowSteps/MergeExistingSignalPlansStepTests.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.WorkflowSteps;
using Utah.Udot.Atspm.InfrastructureTests.Fixtures;
using Utah.Udot.Atspm.Repositories.AggregationRepositories;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.InfrastructureTests.WorkflowSteps
{
    public class MergeExistingSignalPlansStepTests : IClassFixture<EFContextFixture<AggregationContext>>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly AggregationContext _context;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<ISignalTimingPlanRepository> _mockRepo;

        public MergeExistingSignalPlansStepTests(ITestOutputHelper output, EFContextFixture<AggregationContext> fixture)
        {
            _output = output;
            _context = fixture.Context;

            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockRepo = new Mock<ISignalTimingPlanRepository>();

            _mockServiceProvider.Setup(x => x.GetService(typeof(ISignalTimingPlanRepository)))
                .Returns(_mockRepo.Object);

            _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);

            _mockScopeFactory.Setup(x => x.CreateScope())
                .Returns(_mockScope.Object);

            _context.SignalTimingPlans.RemoveRange(_context.SignalTimingPlans);
            _context.SaveChanges();
        }

        [Fact]
        [Trait(nameof(MergeExistingSignalPlansStep), "Process")]
        public async Task Process_SuccessfullyMergesExistingAndNewPlans()
        {
            var startTime = new DateTime(2026, 1, 1, 10, 0, 0);
            var loc = "1001";
            short planId = 1;

            var existing = new SignalTimingPlan { LocationIdentifier = loc, PlanNumber = planId, Start = startTime.AddHours(-1) };
            _context.SignalTimingPlans.Add(existing);
            await _context.SaveChangesAsync();

            var inputPlans = new List<SignalTimingPlan>
        {
            new() { LocationIdentifier = loc, PlanNumber = planId, Start = startTime }
        };

            _mockRepo.Setup(r => r.GetList()).Returns(_context.SignalTimingPlans.AsNoTracking());

            var sut = new MergeExistingSignalPlansStep(_mockScopeFactory.Object, new ExecutionDataflowBlockOptions());

            sut.Post(inputPlans);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                if (sut.TryReceive(out var chunk)) results.Add(chunk);
            }

            var flatResults = results.SelectMany(x => x).ToList();

            Assert.Equal(2, flatResults.Count);
            Assert.True(flatResults.Any(p => p.Start == startTime));
            Assert.True(flatResults.Any(p => p.Start == startTime.AddHours(-1)));
        }

        [Fact]
        [Trait(nameof(MergeExistingSignalPlansStep), "Process")]
        public async Task Process_RespectsTemporalWindow_UsingSqliteFixture()
        {
            var startTime = new DateTime(2026, 8, 1, 10, 0, 0);

            _context.SignalTimingPlans.AddRange(new List<SignalTimingPlan>
        {
            new() { LocationIdentifier = "L1", PlanNumber = 1, Start = startTime.AddHours(5) },
            new() { LocationIdentifier = "L1", PlanNumber = 1, Start = startTime.AddHours(-13) }
        });
            await _context.SaveChangesAsync();

            var inputPlans = new List<SignalTimingPlan>
        {
            new() { LocationIdentifier = "L1", PlanNumber = 1, Start = startTime }
        };

            _mockRepo.Setup(r => r.GetList()).Returns(_context.SignalTimingPlans.AsQueryable());

            var sut = new MergeExistingSignalPlansStep(_mockScopeFactory.Object, new ExecutionDataflowBlockOptions());

            sut.Post(inputPlans);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                if (sut.TryReceive(out var chunk)) results.Add(chunk);
            }

            var flatResults = results.SelectMany(x => x).ToList();

            Assert.Equal(2, flatResults.Count);
            Assert.DoesNotContain(flatResults, p => p.Start == startTime.AddHours(-13));
        }

        [Fact]
        [Trait(nameof(MergeExistingSignalPlansStep), "Process")]
        public async Task Process_HandlesMultipleLocations_DoesNotMixData()
        {
            var startTime = new DateTime(2026, 1, 1);

            _context.SignalTimingPlans.Add(new SignalTimingPlan { LocationIdentifier = "L1", PlanNumber = 1, Start = startTime.AddMinutes(5) });
            await _context.SaveChangesAsync();

            var inputPlans = new List<SignalTimingPlan>
        {
            new() { LocationIdentifier = "L1", PlanNumber = 1, Start = startTime },
            new() { LocationIdentifier = "L2", PlanNumber = 1, Start = startTime }
        };

            _mockRepo.Setup(r => r.GetList()).Returns(_context.SignalTimingPlans.AsNoTracking());
            var sut = new MergeExistingSignalPlansStep(_mockScopeFactory.Object, new ExecutionDataflowBlockOptions());

            sut.Post(inputPlans);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                if (sut.TryReceive(out var chunk)) results.Add(chunk);
            }

            Assert.Equal(2, results.Count);

            var l1Group = results.First(g => g.Any(p => p.LocationIdentifier == "L1")).ToList();
            var l2Group = results.First(g => g.Any(p => p.LocationIdentifier == "L2")).ToList();

            Assert.Equal(2, l1Group.Count);
            Assert.Single(l2Group);
        }

        [Fact]
        [Trait(nameof(MergeExistingSignalPlansStep), "Process")]
        public async Task Process_EmptyInput_DoesNotQueryDatabase()
        {
            var sut = new MergeExistingSignalPlansStep(_mockScopeFactory.Object, new ExecutionDataflowBlockOptions());

            sut.Post(Enumerable.Empty<SignalTimingPlan>());
            sut.Complete();

            // With yield break, the block produces nothing, so OutputAvailableAsync evaluates to false
            var resultsExist = await sut.OutputAvailableAsync();
            Assert.False(resultsExist);
            _mockRepo.Verify(r => r.GetList(), Times.Never);
        }

        // ==========================================
        // TEST 5: THE CHAINED EMPTY-INPUT TIMEOUT VERIFIER
        // ==========================================
        [Fact]
        [Trait(nameof(MergeExistingSignalPlansStep), "Faults")]
        public async Task Process_EmptyInput_DoesItDeadlockDownstream()
        {
            var sut = new MergeExistingSignalPlansStep(_mockScopeFactory.Object, new ExecutionDataflowBlockOptions());

            var downstreamMockBlock = new BufferBlock<IEnumerable<SignalTimingPlan>>();
            sut.LinkTo(downstreamMockBlock, new DataflowLinkOptions { PropagateCompletion = true });

            sut.Post(Enumerable.Empty<SignalTimingPlan>());
            sut.Complete();

            try
            {
                // With yield break, completion propagates cleanly to the downstream target block
                await downstreamMockBlock.Completion.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                Assert.Fail("The empty data payload execution loop stalled the downstream completion tracking handles.");
            }

            Assert.True(downstreamMockBlock.Completion.IsCompletedSuccessfully);
        }

        public void Dispose()
        {
        }
    }
}
