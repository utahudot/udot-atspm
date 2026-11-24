#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.InfrastructureTests.WorkflowSteps/SaveSignalTimingPlansStepTests.cs
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.ATSPM.Infrastructure.Workflows;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.WorkflowSteps;
using Utah.Udot.Atspm.InfrastructureTests.Fixtures;
using Utah.Udot.Atspm.Repositories.AggregationRepositories;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.InfrastructureTests.WorkflowSteps
{
    /// <summary>
    /// Unit and integration tests for <see cref="SaveSignalTimingPlansStep"/> and <see cref="SignalTimingPlansWorkflow"/>.
    /// </summary>
    public class SaveSignalTimingPlansStepTests : WorkflowStepTestBase<SaveSignalTimingPlansStep, SaveSignalTimingPlansTestData, Location, IEnumerable<SignalTimingPlan>, IEnumerable<SignalTimingPlan>>, IClassFixture<EFContextFixture<AggregationContext>>
    {
        private readonly AggregationContext _context;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<ISignalTimingPlanRepository> _mockRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveSignalTimingPlansStepTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper.</param>
        /// <param name="testLocationFixture">The test location fixture.</param>
        /// <param name="fixture">The database context fixture.</param>
        public SaveSignalTimingPlansStepTests(
            ITestOutputHelper output,
            TestLocationFixture testLocationFixture,
            EFContextFixture<AggregationContext> fixture)
            : base(output, testLocationFixture)
        {
            _context = fixture.Context;
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockRepo = new Mock<ISignalTimingPlanRepository>();

            _mockServiceProvider.Setup(x => x.GetService(typeof(ISignalTimingPlanRepository))).Returns(_mockRepo.Object);
            _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
            _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);

            _context.SignalTimingPlans.RemoveRange(_context.SignalTimingPlans);
            _context.SaveChanges();

            _mockRepo.Setup(r => r.GetList()).Returns(() => _context.SignalTimingPlans.AsQueryable());
        }

        /// <inheritdoc/>
        protected override IEnumerable<SignalTimingPlan> DefaultTestInput => new List<SignalTimingPlan>();

        /// <inheritdoc/>
        protected override SaveSignalTimingPlansStep CreateStep(
            Location config,
            IEnumerable<SignalTimingPlan> input,
            IEnumerable<SignalTimingPlan> expected)
        {
            return new SaveSignalTimingPlansStep(_mockScopeFactory.Object);
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<SignalTimingPlan>> ExecuteStepAsync(
            SaveSignalTimingPlansStep step,
            Location config,
            IEnumerable<SignalTimingPlan> input,
            CancellationToken cancelToken = default)
        {
            if (cancelToken.IsCancellationRequested)
            {
                await Task.FromCanceled(cancelToken);
            }

            _context.SignalTimingPlans.RemoveRange(_context.SignalTimingPlans);
            _context.SaveChanges();
            ConfigurePersistence();

            step.Post(input);
            step.Complete();
            while (await step.OutputAvailableAsync(cancelToken))
            {
                while (step.TryReceive(out _)) { }
            }

            await step.Completion;

            return _context.SignalTimingPlans
                .AsNoTracking()
                .OrderBy(p => p.Start)
                .ToList();
        }

        private void ConfigurePersistence()
        {
            _mockRepo.Setup(r => r.LookupAsync(It.IsAny<SignalTimingPlan>()))
                .ReturnsAsync((SignalTimingPlan p) => _context.SignalTimingPlans.FirstOrDefault(x => x.LocationIdentifier == p.LocationIdentifier && x.PlanNumber == p.PlanNumber && x.Start == p.Start));

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<SignalTimingPlan>()))
                .Callback<SignalTimingPlan>(p =>
                {
                    _context.SignalTimingPlans.Add(p);
                    _context.SaveChanges();
                })
                .Returns(Task.CompletedTask);

            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<SignalTimingPlan>()))
                .Callback<SignalTimingPlan>(p =>
                {
                    _context.SignalTimingPlans.Update(p);
                    _context.SaveChanges();
                })
                .Returns(Task.CompletedTask);
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
                Assert.Equal(expectedList[i].End > expectedList[i].Start, actualList[i].Valid);
            }
        }

        /// <summary>
        /// Verifies that a new plan is added when LookupAsync returns null.
        /// </summary>
        [Fact]
        [Trait(nameof(SaveSignalTimingPlansStep), "Process")]
        public async Task Process_AddsNewPlan_WhenLookupReturnsNull()
        {
            var plan = new SignalTimingPlan { LocationIdentifier = "L1", PlanNumber = 1, Start = DateTime.Now };
            var input = new List<SignalTimingPlan> { plan };

            _mockRepo.Setup(r => r.LookupAsync(It.IsAny<SignalTimingPlan>())).ReturnsAsync((SignalTimingPlan)null);

            var sut = new SaveSignalTimingPlansStep(_mockScopeFactory.Object);

            sut.Post(input);
            sut.Complete();

            var results = new List<SignalTimingPlan>();
            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out var p)) results.Add(p);
            }

            _mockRepo.Verify(r => r.AddAsync(It.Is<SignalTimingPlan>(p => p.LocationIdentifier == "L1" && p.PlanNumber == 1 && p.Start == plan.Start)), Times.Once);
            Assert.Single(results);
        }

        /// <summary>
        /// Verifies that an existing plan's End timestamp is updated when a subsequent plan arrives.
        /// </summary>
        [Fact]
        [Trait(nameof(SaveSignalTimingPlansStep), "Process")]
        public async Task Process_UpdatesExistingPlan_WhenSubsequentPlanArrives()
        {
            var existingStart = new DateTime(2026, 7, 14, 6, 0, 0);
            var nextStart = new DateTime(2026, 7, 14, 9, 15, 0);

            var existing = new SignalTimingPlan { LocationIdentifier = "L1", PlanNumber = 1, Start = existingStart, End = DateTime.MinValue };
            _context.SignalTimingPlans.Add(existing);
            await _context.SaveChangesAsync();

            var nextPlan = new SignalTimingPlan { LocationIdentifier = "L1", PlanNumber = 7, Start = nextStart, End = DateTime.MinValue };

            _mockRepo.Setup(r => r.LookupAsync(It.Is<SignalTimingPlan>(p => p.PlanNumber == 1 && p.Start == existingStart))).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.LookupAsync(It.Is<SignalTimingPlan>(p => p.PlanNumber == 7 && p.Start == nextStart))).ReturnsAsync((SignalTimingPlan)null);

            var sut = new SaveSignalTimingPlansStep(_mockScopeFactory.Object);

            sut.Post(new List<SignalTimingPlan> { nextPlan });
            sut.Complete();

            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out _)) { }
            }

            Assert.Equal(nextStart, existing.End);
            _mockRepo.Verify(r => r.UpdateAsync(existing), Times.Once);
            _mockRepo.Verify(r => r.AddAsync(It.Is<SignalTimingPlan>(p => p.PlanNumber == 7 && p.Start == nextStart)), Times.Once);
        }

        /// <summary>
        /// Verifies that out-of-order batches reconcile into contiguous intervals and produce the exact report timeline.
        /// </summary>
        [Fact]
        [Trait(nameof(SaveSignalTimingPlansStep), "Process")]
        public async Task Process_ReconcilesOutOfOrderBatches_IntoContiguousTimeline()
        {
            var loc = "7115";
            var t0600 = new DateTime(2026, 7, 14, 6, 0, 0);
            var t0915 = new DateTime(2026, 7, 14, 9, 15, 0);
            var t1500 = new DateTime(2026, 7, 14, 15, 0, 0);
            var t1640 = new DateTime(2026, 7, 14, 16, 40, 0);
            var t1750 = new DateTime(2026, 7, 14, 17, 50, 0);
            var t1900 = new DateTime(2026, 7, 14, 19, 0, 0);
            var t1922 = new DateTime(2026, 7, 14, 19, 22, 5, 400);
            var t2130 = new DateTime(2026, 7, 14, 21, 30, 0);

            _mockRepo.Setup(r => r.LookupAsync(It.IsAny<SignalTimingPlan>()))
                .ReturnsAsync((SignalTimingPlan p) => _context.SignalTimingPlans.FirstOrDefault(x => x.LocationIdentifier == p.LocationIdentifier && x.PlanNumber == p.PlanNumber && x.Start == p.Start));

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<SignalTimingPlan>()))
                .Callback<SignalTimingPlan>(p =>
                {
                    _context.SignalTimingPlans.Add(p);
                    _context.SaveChanges();
                })
                .Returns(Task.CompletedTask);

            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<SignalTimingPlan>()))
                .Callback<SignalTimingPlan>(p =>
                {
                    _context.SignalTimingPlans.Update(p);
                    _context.SaveChanges();
                })
                .Returns(Task.CompletedTask);

            var batchLate = new List<SignalTimingPlan>
            {
                new() { LocationIdentifier = loc, PlanNumber = 7, Start = t1922 },
                new() { LocationIdentifier = loc, PlanNumber = 254, Start = t2130 }
            };

            var batchEarly = new List<SignalTimingPlan>
            {
                new() { LocationIdentifier = loc, PlanNumber = 1, Start = t0600 },
                new() { LocationIdentifier = loc, PlanNumber = 7, Start = t0915 },
                new() { LocationIdentifier = loc, PlanNumber = 13, Start = t1500 },
                new() { LocationIdentifier = loc, PlanNumber = 13, Start = t1750 }
            };

            var batchMiddle = new List<SignalTimingPlan>
            {
                new() { LocationIdentifier = loc, PlanNumber = 14, Start = t1640 },
                new() { LocationIdentifier = loc, PlanNumber = 7, Start = t1900 }
            };

            var sut = new SaveSignalTimingPlansStep(_mockScopeFactory.Object);

            sut.Post(batchLate);
            sut.Post(batchEarly);
            sut.Post(batchMiddle);
            sut.Complete();

            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out _)) { }
            }

            var stored = _context.SignalTimingPlans
                .AsNoTracking()
                .Where(p => p.LocationIdentifier == loc)
                .OrderBy(p => p.Start)
                .ToList();

            Assert.Equal(8, stored.Count);

            var planService = new PlanService();
            var reportPlans = planService.GetPlans(loc, new DateTime(2026, 7, 14, 6, 0, 0), new DateTime(2026, 7, 14, 20, 0, 0), stored);

            Assert.Equal(6, reportPlans.Count);
            Assert.Equal(new[] { "1", "7", "13", "14", "13", "7" }, reportPlans.Select(p => p.PlanNumber).ToArray());
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(50000, false)]
        [InlineData(50000, true)]
        public async Task Replay_KeepsLastControllerChangeAtSameTimestamp(int batchSize, bool repeatFirstChange)
        {
            const string location = "7227";
            var start = new DateTime(2026, 11, 13, 12, 4, 23);
            var end = start.AddSeconds(6.3);
            _context.SignalTimingPlans.Add(new SignalTimingPlan { LocationIdentifier = location, PlanNumber = 255, Start = start, End = end });
            await _context.SaveChangesAsync();
            ConfigurePersistence();

            var workflow = new SignalTimingPlansWorkflow(_mockScopeFactory.Object, batchSize, parallelProcesses: 4);
            await workflow.Initialize();
            await workflow.Input.SendAsync(new IndianaEvent { LocationIdentifier = location, EventCode = 131, EventParam = 254, Timestamp = start });
            await workflow.Input.SendAsync(new IndianaEvent { LocationIdentifier = location, EventCode = 131, EventParam = 255, Timestamp = start });
            if (repeatFirstChange)
                await workflow.Input.SendAsync(new IndianaEvent { LocationIdentifier = location, EventCode = 131, EventParam = 254, Timestamp = start });
            await workflow.Input.SendAsync(new IndianaEvent { LocationIdentifier = location, EventCode = 131, EventParam = 7, Timestamp = end });
            workflow.Input.Complete();
            while (await workflow.Output.OutputAvailableAsync())
            {
                while (workflow.Output.TryReceive(out _)) { }
            }
            await workflow.Output.Completion;

            var rows = await _context.SignalTimingPlans.AsNoTracking().ToListAsync();
            var (winner, other) = repeatFirstChange ? (254, 255) : (255, 254);
            Assert.Equal(start, rows.Single(plan => plan.PlanNumber == other).End);
            Assert.Equal(end, rows.Single(plan => plan.PlanNumber == winner).End);
            Assert.Equal(winner.ToString(), Assert.Single(new PlanService().GetPlans(location, start, end, rows)).PlanNumber);
        }

        [Fact]
        public async Task Workflow_SavesOtherLocationsWhileKeepingBatchesInLocationOrder()
        {
            var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var otherStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var timeout = TimeSpan.FromSeconds(10);
            _mockRepo.Setup(r => r.GetList()).Returns((IQueryable<SignalTimingPlan>)null);
            _mockRepo.Setup(r => r.LookupAsync(It.IsAny<SignalTimingPlan>())).ReturnsAsync((SignalTimingPlan)null);
            _mockRepo.Setup(r => r.AddAsync(It.IsAny<SignalTimingPlan>())).Returns((SignalTimingPlan plan) => plan.PlanNumber switch
            {
                1 => WaitForRelease(firstStarted, releaseFirst),
                2 => WaitForRelease(secondStarted),
                3 => WaitForRelease(otherStarted),
                _ => throw new InvalidOperationException()
            });

            var workflow = new SignalTimingPlansWorkflow(_mockScopeFactory.Object, batchSize: 1, parallelProcesses: 2);
            await workflow.Initialize();
            workflow.Output.LinkTo(DataflowBlock.NullTarget<SignalTimingPlan>());
            var start = new DateTime(2026, 11, 13, 12, 4, 23);
            try
            {
                await workflow.Input.SendAsync(new IndianaEvent { LocationIdentifier = "L1", EventCode = 131, EventParam = 1, Timestamp = start });
                await firstStarted.Task.WaitAsync(timeout);
                await workflow.Input.SendAsync(new IndianaEvent { LocationIdentifier = "L1", EventCode = 131, EventParam = 2, Timestamp = start });
                await workflow.Input.SendAsync(new IndianaEvent { LocationIdentifier = "L2", EventCode = 131, EventParam = 3, Timestamp = start });
                await otherStarted.Task.WaitAsync(timeout);
                Assert.False(secondStarted.Task.IsCompleted);
            }
            finally
            {
                releaseFirst.TrySetResult();
                workflow.Input.Complete();
            }

            await workflow.Output.Completion.WaitAsync(timeout);
            Assert.True(secondStarted.Task.IsCompletedSuccessfully);

            static Task WaitForRelease(TaskCompletionSource started, TaskCompletionSource release = null)
            {
                started.SetResult();
                return release?.Task ?? Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Replay_UsesPostgreSqlPrecisionBeforeMergingKeys()
        {
            const string location = "7178";
            var storedStart = new DateTime(2026, 11, 10, 21, 49, 22).AddTicks(6999990);
            var next = storedStart.AddMinutes(1);
            using var postgres = new AggregationContext(new DbContextOptionsBuilder<AggregationContext>()
                .UseNpgsql("Host=localhost;Database=timing_plan_test")
                .Options);
            // Use the PostgreSQL provider for precision and the fixture repository for persistence.
            _mockServiceProvider.Setup(x => x.GetService(typeof(AggregationContext))).Returns(postgres);
            _context.SignalTimingPlans.Add(new SignalTimingPlan { LocationIdentifier = location, PlanNumber = 13, Start = storedStart, End = next.AddMinutes(1) });
            await _context.SaveChangesAsync();
            ConfigurePersistence();

            var step = new SaveSignalTimingPlansStep(_mockScopeFactory.Object);
            step.Post(new[]
            {
                new SignalTimingPlan { LocationIdentifier = location, PlanNumber = 13, Start = storedStart.AddTicks(9) },
                new SignalTimingPlan { LocationIdentifier = location, PlanNumber = 13, Start = next.AddTicks(9) }
            });
            step.Complete();
            while (await step.OutputAvailableAsync())
            {
                while (step.TryReceive(out _)) { }
            }
            await step.Completion;

            var rows = await _context.SignalTimingPlans.AsNoTracking().OrderBy(plan => plan.Start).ToListAsync();
            Assert.Collection(rows,
                plan => Assert.Equal((storedStart, next), (plan.Start, plan.End)),
                plan => Assert.Equal((next, DateTime.MinValue), (plan.Start, plan.End)));
        }

        /// <summary>
        /// Runs the full <see cref="SignalTimingPlansWorkflow"/> against the 7115 raw CSV file in reverse batch order and verifies the stored rows and PlanService timeline.
        /// </summary>
        [Fact]
        [Trait(nameof(SignalTimingPlansWorkflow), "EndToEnd")]
        public async Task Workflow_EndToEnd_With7115RawCsv_ProducesExactTimelineInAnyBatchOrder()
        {
            var csvPath = @"c:\Users\christianbaker\Downloads\7115_IndianaEvent_2026-07-14T00_00_00.csv";
            if (!File.Exists(csvPath))
            {
                return;
            }

            _mockRepo.Setup(r => r.LookupAsync(It.IsAny<SignalTimingPlan>()))
                .ReturnsAsync((SignalTimingPlan p) => _context.SignalTimingPlans.FirstOrDefault(x => x.LocationIdentifier == p.LocationIdentifier && x.PlanNumber == p.PlanNumber && x.Start == p.Start));

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<SignalTimingPlan>()))
                .Callback<SignalTimingPlan>(p =>
                {
                    _context.SignalTimingPlans.Add(p);
                    _context.SaveChanges();
                })
                .Returns(Task.CompletedTask);

            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<SignalTimingPlan>()))
                .Callback<SignalTimingPlan>(p =>
                {
                    _context.SignalTimingPlans.Update(p);
                    _context.SaveChanges();
                })
                .Returns(Task.CompletedTask);

            var allEvents = new List<IndianaEvent>(540000);
            using (var reader = new StreamReader(csvPath))
            {
                await reader.ReadLineAsync();
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3)
                    {
                        allEvents.Add(new IndianaEvent
                        {
                            LocationIdentifier = "7115",
                            EventCode = short.Parse(parts[0], CultureInfo.InvariantCulture),
                            EventParam = short.Parse(parts[1], CultureInfo.InvariantCulture),
                            Timestamp = DateTime.Parse(parts[2], CultureInfo.InvariantCulture)
                        });
                    }
                }
            }

            var reverseBatchedEvents = allEvents
                .Chunk(50000)
                .Reverse()
                .SelectMany(b => b)
                .ToList();

            var workflow = new SignalTimingPlansWorkflow(_mockScopeFactory.Object, batchSize: 50000, parallelProcesses: 50);
            await workflow.Initialize();

            foreach (var ev in reverseBatchedEvents)
            {
                await workflow.Input.SendAsync(ev);
            }
            workflow.Input.Complete();

            while (await workflow.Output.OutputAvailableAsync())
            {
                while (workflow.Output.TryReceive(out _)) { }
            }
            await workflow.Output.Completion;

            var stored = _context.SignalTimingPlans
                .AsNoTracking()
                .Where(p => p.LocationIdentifier == "7115")
                .OrderBy(p => p.Start)
                .ToList();

            Output.WriteLine("STORED DATABASE ROWS:");
            Output.WriteLine("LocationIdentifier | PlanNumber | Start                   | End                     | Valid");
            Output.WriteLine("-------------------------------------------------------------------------------------------");
            foreach (var row in stored)
            {
                Output.WriteLine($"{row.LocationIdentifier,-18} | {row.PlanNumber,-10} | {row.Start:yyyy-MM-dd HH:mm:ss.fff} | {row.End:yyyy-MM-dd HH:mm:ss.fff} | {row.Valid}");
            }

            var planService = new PlanService();
            var reportWindowStart = new DateTime(2026, 7, 14, 6, 0, 0);
            var reportWindowEnd = new DateTime(2026, 7, 14, 20, 0, 0);
            var reportPlans = planService.GetPlans("7115", reportWindowStart, reportWindowEnd, stored);

            Output.WriteLine("");
            Output.WriteLine("SPLIT MONITOR / PLANSERVICE TIMELINE (6:00 AM - 8:00 PM):");
            Output.WriteLine("PlanNumber | Start               | End");
            Output.WriteLine("------------------------------------------------------");
            foreach (var p in reportPlans)
            {
                Output.WriteLine($"{p.PlanNumber,-10} | {p.Start:yyyy-MM-dd HH:mm:ss} | {p.End:yyyy-MM-dd HH:mm:ss}");
            }

            Assert.Equal(13, stored.Count);
            Assert.Equal(6, reportPlans.Count);

            Assert.Equal("1", reportPlans[0].PlanNumber);
            Assert.Equal(new DateTime(2026, 7, 14, 6, 0, 0), reportPlans[0].Start);
            Assert.Equal(new DateTime(2026, 7, 14, 9, 15, 0), reportPlans[0].End);

            Assert.Equal("7", reportPlans[1].PlanNumber);
            Assert.Equal(new DateTime(2026, 7, 14, 9, 15, 0), reportPlans[1].Start);
            Assert.Equal(new DateTime(2026, 7, 14, 15, 0, 0), reportPlans[1].End);

            Assert.Equal("13", reportPlans[2].PlanNumber);
            Assert.Equal(new DateTime(2026, 7, 14, 15, 0, 0), reportPlans[2].Start);
            Assert.Equal(new DateTime(2026, 7, 14, 16, 40, 0), reportPlans[2].End);

            Assert.Equal("14", reportPlans[3].PlanNumber);
            Assert.Equal(new DateTime(2026, 7, 14, 16, 40, 0), reportPlans[3].Start);
            Assert.Equal(new DateTime(2026, 7, 14, 17, 50, 0), reportPlans[3].End);

            Assert.Equal("13", reportPlans[4].PlanNumber);
            Assert.Equal(new DateTime(2026, 7, 14, 17, 50, 0), reportPlans[4].Start);
            Assert.Equal(new DateTime(2026, 7, 14, 19, 0, 0), reportPlans[4].End);

            Assert.Equal("7", reportPlans[5].PlanNumber);
            Assert.Equal(new DateTime(2026, 7, 14, 19, 0, 0), reportPlans[5].Start);
            Assert.Equal(new DateTime(2026, 7, 14, 20, 0, 0), reportPlans[5].End);
        }

        /// <summary>
        /// Verifies that no repository calls are made when an existing plan is already identical.
        /// </summary>
        [Fact]
        [Trait(nameof(SaveSignalTimingPlansStep), "Process")]
        public async Task Process_DoesNothing_WhenExistingPlanIsIdentical()
        {
            var time = DateTime.Now;
            var existing = new SignalTimingPlan { LocationIdentifier = "L1", PlanNumber = 1, Start = time, End = time.AddMinutes(10) };
            _context.SignalTimingPlans.Add(existing);
            await _context.SaveChangesAsync();

            var identicalInput = new SignalTimingPlan { LocationIdentifier = "L1", PlanNumber = 1, Start = time, End = time.AddMinutes(10) };

            _mockRepo.Setup(r => r.LookupAsync(It.IsAny<SignalTimingPlan>())).ReturnsAsync(existing);

            var sut = new SaveSignalTimingPlansStep(_mockScopeFactory.Object);

            sut.Post(new List<SignalTimingPlan> { identicalInput });
            sut.Complete();

            while (await sut.OutputAvailableAsync())
            {
                while (sut.TryReceive(out _)) { }
            }

            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<SignalTimingPlan>()), Times.Never);
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<SignalTimingPlan>()), Times.Never);
        }

        /// <summary>
        /// Verifies that empty input completes without creating a service scope.
        /// </summary>
        [Fact]
        [Trait(nameof(SaveSignalTimingPlansStep), "Process")]
        public async Task Process_HandlesEmptyInput_WithoutCreatingScope()
        {
            var sut = new SaveSignalTimingPlansStep(_mockScopeFactory.Object);

            sut.Post(Enumerable.Empty<SignalTimingPlan>());
            sut.Complete();

            var resultsExist = await sut.OutputAvailableAsync();
            Assert.False(resultsExist);
            _mockScopeFactory.Verify(x => x.CreateScope(), Times.Never);
        }
    }
}
