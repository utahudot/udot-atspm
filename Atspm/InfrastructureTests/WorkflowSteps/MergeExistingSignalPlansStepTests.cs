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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.InfrastructureTests.Fixtures;
using Utah.Udot.Atspm.Repositories.AggregationRepositories;
using Utah.Udot.ATSPM.Infrastructure.Workflows;
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

            // 1. Setup DI for the Repository
            _mockServiceProvider.Setup(x => x.GetService(typeof(ISignalTimingPlanRepository)))
                .Returns(_mockRepo.Object);

            // 2. Setup the Scope to return the ServiceProvider
            _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);

            // 3. FIX: Mock CreateScope. 
            // Even though the code calls CreateAsyncScope(), the extension method 
            // actually executes CreateScope() under the hood.
            _mockScopeFactory.Setup(x => x.CreateScope())
                .Returns(_mockScope.Object);

            // Cleanup SQLite table between tests
            _context.SignalTimingPlans.RemoveRange(_context.SignalTimingPlans);
            _context.SaveChanges();
        }

        [Fact]
        [Trait(nameof(MergeExistingSignalPlansStep), "Process")]
        public async Task Process_SuccessfullyMergesExistingAndNewPlans()
        {
            // Arrange
            var startTime = new DateTime(2026, 1, 1, 10, 0, 0);
            var loc = "1001";
            short planId = 1;

            // Add existing plan to SQLite
            var existing = new SignalTimingPlan { LocationIdentifier = loc, PlanNumber = planId, Start = startTime.AddHours(-1) };
            _context.SignalTimingPlans.Add(existing);
            await _context.SaveChangesAsync();

            // Input plan (New)
            var inputPlans = new List<SignalTimingPlan>
            {
                new() { LocationIdentifier = loc, PlanNumber = planId, Start = startTime }
            };

            // Point the mocked repo to the SQLite context
            _mockRepo.Setup(r => r.GetList()).Returns(_context.SignalTimingPlans.AsNoTracking());

            var sut = new MergeExistingSignalPlansStep(_mockScopeFactory.Object, new ExecutionDataflowBlockOptions());

            // Act
            sut.Post(inputPlans);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                if (sut.TryReceive(out var chunk)) results.Add(chunk);
            }

            // Assert
            var flatResults = results.SelectMany(x => x).ToList();

            // Total should be 2 (1 from DB, 1 from input)
            Assert.Equal(2, flatResults.Count);
            Assert.True(flatResults.Any(p => p.Start == startTime));
            Assert.True(flatResults.Any(p => p.Start == startTime.AddHours(-1)));
        }

        [Fact]
        [Trait(nameof(MergeExistingSignalPlansStep), "Process")]
        public async Task Process_RespectsTemporalWindow_UsingSqliteFixture()
        {
            // Arrange
            var startTime = new DateTime(2026, 8, 1, 10, 0, 0);

            // Seed one inside window (+5h), one outside (-13h)
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

            // Act
            sut.Post(inputPlans);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                if (sut.TryReceive(out var chunk)) results.Add(chunk);
            }

            // Assert
            var flatResults = results.SelectMany(x => x).ToList();

            // Expected: Input (1) + Inside Window (1) = 2. Outside window is filtered.
            Assert.Equal(2, flatResults.Count);
            Assert.DoesNotContain(flatResults, p => p.Start == startTime.AddHours(-13));
        }

        [Fact]
        [Trait(nameof(MergeExistingSignalPlansStep), "Process")]
        public async Task Process_HandlesMultipleLocations_DoesNotMixData()
        {
            // Arrange
            var startTime = new DateTime(2026, 1, 1);

            // Seed DB with data for Location 1 ONLY
            _context.SignalTimingPlans.Add(new SignalTimingPlan { LocationIdentifier = "L1", PlanNumber = 1, Start = startTime.AddMinutes(5) });
            await _context.SaveChangesAsync();

            // Input data for both Location 1 and Location 2
            var inputPlans = new List<SignalTimingPlan>
            {
                new() { LocationIdentifier = "L1", PlanNumber = 1, Start = startTime },
                new() { LocationIdentifier = "L2", PlanNumber = 1, Start = startTime }
            };

            _mockRepo.Setup(r => r.GetList()).Returns(_context.SignalTimingPlans.AsNoTracking());
            var sut = new MergeExistingSignalPlansStep(_mockScopeFactory.Object, new ExecutionDataflowBlockOptions());

            // Act
            sut.Post(inputPlans);
            sut.Complete();

            var results = new List<IEnumerable<SignalTimingPlan>>();
            while (await sut.OutputAvailableAsync())
            {
                if (sut.TryReceive(out var chunk)) results.Add(chunk);
            }

            // Assert
            Assert.Equal(2, results.Count); // Two groups: (L1, P1) and (L2, P1)

            var l1Group = results.First(g => g.Any(p => p.LocationIdentifier == "L1")).ToList();
            var l2Group = results.First(g => g.Any(p => p.LocationIdentifier == "L2")).ToList();

            Assert.Equal(2, l1Group.Count); // Input + DB
            Assert.Single(l2Group); // Input Only - No bleed from L1 DB data
        }

        [Fact]
        [Trait(nameof(MergeExistingSignalPlansStep), "Process")]
        public async Task Process_EmptyInput_DoesNotQueryDatabase()
        {
            // Arrange
            var sut = new MergeExistingSignalPlansStep(_mockScopeFactory.Object, new ExecutionDataflowBlockOptions());

            // Act
            sut.Post(Enumerable.Empty<SignalTimingPlan>());
            sut.Complete();

            // Assert
            var resultsExist = await sut.OutputAvailableAsync();
            Assert.False(resultsExist);
            _mockRepo.Verify(r => r.GetList(), Times.Never);
        }

        public void Dispose()
        {
            // The fixture handles the connection disposal
        }
    }
}
