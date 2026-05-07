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
    public class SaveSignalTimingPlansStepTests : IClassFixture<EFContextFixture<AggregationContext>>
    {
        private readonly AggregationContext _context;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<ISignalTimingPlanRepository> _mockRepo;

        public SaveSignalTimingPlansStepTests(ITestOutputHelper output, EFContextFixture<AggregationContext> fixture)
        {
            _context = fixture.Context;
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockRepo = new Mock<ISignalTimingPlanRepository>();

            _mockServiceProvider.Setup(x => x.GetService(typeof(ISignalTimingPlanRepository))).Returns(_mockRepo.Object);
            _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
            _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);

            // Reset SQLite table
            _context.SignalTimingPlans.RemoveRange(_context.SignalTimingPlans);
            _context.SaveChanges();
        }

        [Fact]
        [Trait(nameof(SaveSignalTimingPlansStep), "Process")]
        public async Task Process_AddsNewPlan_WhenLookupReturnsNull()
        {
            // Arrange
            var plan = new SignalTimingPlan { LocationIdentifier = "L1", PlanNumber = 1, Start = DateTime.Now };
            var input = new List<SignalTimingPlan> { plan };

            // Setup: Lookup returns null, suggesting it's new
            _mockRepo.Setup(r => r.LookupAsync(It.IsAny<SignalTimingPlan>())).ReturnsAsync((SignalTimingPlan)null);

            var sut = new SaveSignalTimingPlansStep(_mockScopeFactory.Object);

            // Act
            sut.Post(input);
            sut.Complete();

            var results = new List<SignalTimingPlan>();
            while (await sut.OutputAvailableAsync())
            {
                // Note: Receiving SignalTimingPlan directly (flattened)
                while (sut.TryReceive(out var p)) results.Add(p);
            }

            // Assert
            _mockRepo.Verify(r => r.AddAsync(plan), Times.Once);
            Assert.Single(results);
        }

        [Fact]
        [Trait(nameof(SaveSignalTimingPlansStep), "Process")]
        public async Task Process_UpdatesExistingPlan_WhenEndTimesDiffer()
        {
            // Arrange
            var existingStart = DateTime.Now.AddHours(-1);
            var newEnd = DateTime.Now;
            short planNum = 5;

            var existing = new SignalTimingPlan { LocationIdentifier = "L1", PlanNumber = planNum, Start = existingStart, End = DateTime.MinValue };
            var updateRequest = new SignalTimingPlan { LocationIdentifier = "L1", PlanNumber = planNum, Start = existingStart, End = newEnd };

            var input = new List<SignalTimingPlan> { updateRequest };

            _mockRepo.Setup(r => r.LookupAsync(It.IsAny<SignalTimingPlan>())).ReturnsAsync(existing);

            var sut = new SaveSignalTimingPlansStep(_mockScopeFactory.Object);

            // Act
            sut.Post(input);
            sut.Complete();

            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out var _)) { }
            }

            // Assert
            Assert.Equal(newEnd, existing.End);
            _mockRepo.Verify(r => r.UpdateAsync(existing), Times.Once);
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<SignalTimingPlan>()), Times.Never);
        }

        [Fact]
        [Trait(nameof(SaveSignalTimingPlansStep), "Process")]
        public async Task Process_DoesNothing_WhenExistingPlanIsIdentical()
        {
            // Arrange
            var time = DateTime.Now;
            var existing = new SignalTimingPlan { LocationIdentifier = "L1", Start = time, End = time.AddMinutes(10) };
            var identicalInput = new SignalTimingPlan { LocationIdentifier = "L1", Start = time, End = time.AddMinutes(10) };

            _mockRepo.Setup(r => r.LookupAsync(It.IsAny<SignalTimingPlan>())).ReturnsAsync(existing);

            var sut = new SaveSignalTimingPlansStep(_mockScopeFactory.Object);

            // Act
            sut.Post(new List<SignalTimingPlan> { identicalInput });
            sut.Complete();

            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out var _)) { }
            }

            // Assert
            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<SignalTimingPlan>()), Times.Never);
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<SignalTimingPlan>()), Times.Never);
        }

        [Fact]
        [Trait(nameof(SaveSignalTimingPlansStep), "Process")]
        public async Task Process_HandlesEmptyInput_WithoutCreatingScope()
        {
            // Arrange
            var sut = new SaveSignalTimingPlansStep(_mockScopeFactory.Object);

            // Act
            sut.Post(Enumerable.Empty<SignalTimingPlan>());
            sut.Complete();

            // Assert
            var resultsExist = await sut.OutputAvailableAsync();
            Assert.False(resultsExist);
            _mockScopeFactory.Verify(x => x.CreateScope(), Times.Never);
        }
    }
}
