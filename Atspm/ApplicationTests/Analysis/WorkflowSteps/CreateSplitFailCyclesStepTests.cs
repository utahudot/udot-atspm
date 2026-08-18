#region license
/// Copyright 2026 Utah Departement of Transportation
/// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/CreateSplitFailCyclesStepTests.cs
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
/// 
/// http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Attributes;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    /// <summary>
    /// Unit tests for the CreateSplitFailCyclesStep workflow step.
    /// </summary>
    public class CreateSplitFailCyclesStepTests : WorkflowStepTestBase<CreateSplitFailCyclesStep, CreateSplitFailCyclesTestData, Location, IEnumerable<IndianaEvent>, Tuple<Location, IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>>>
    {
        /// <summary>
        /// Initializes a new instance of the CreateSplitFailCyclesStepTests class.
        /// </summary>
        /// <param name="output">The xUnit test output helper.</param>
        /// <param name="testLocationFixture">The test location class fixture.</param>
        public CreateSplitFailCyclesStepTests(ITestOutputHelper output, TestLocationFixture testLocationFixture) : base(output, testLocationFixture) { }

        /// <inheritdoc/>
        [Theory(Skip = "No JSON test data files available yet.")]
        [AnalysisTestData<CreateSplitFailCyclesTestData>]
        public override Task ExecuteStepFromFileTest(Location config, IEnumerable<IndianaEvent> input, Tuple<Location, IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>> expected)
        {
            return base.ExecuteStepFromFileTest(config, input, expected);
        }

        /// <inheritdoc/>
        protected override CreateSplitFailCyclesStep CreateStep(Location config, IEnumerable<IndianaEvent> input, Tuple<Location, IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>> expected)
        {
            return new CreateSplitFailCyclesStep();
        }

        /// <inheritdoc/>
        protected override Task<Tuple<Location, IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>>> ExecuteStepAsync(CreateSplitFailCyclesStep step, Location config, IEnumerable<IndianaEvent> input)
        {
            return step.ExecuteAsync(Tuple.Create(config, input));
        }

        /// <summary>
        /// Verifies that processing is cancelled when a cancelled token is passed.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailCyclesStep), "Cancellation")]
        public async Task Process_Cancellation_ThrowsTaskCanceledException()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var sut = new CreateSplitFailCyclesStep();
            var input = Tuple.Create(TestLocation, (IEnumerable<IndianaEvent>)new List<IndianaEvent>());

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExecuteAsync(input, cts.Token));
        }

        /// <summary>
        /// Verifies that empty input event lists return empty cycle results.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailCyclesStep), "EmptyEvents")]
        public async Task Process_EmptyEvents_ReturnsEmptyCycles()
        {
            var sut = new CreateSplitFailCyclesStep();
            var input = Tuple.Create(TestLocation, (IEnumerable<IndianaEvent>)new List<IndianaEvent>());

            var result = await sut.ExecuteAsync(input);

            Assert.NotNull(result);
            Assert.Equal(TestLocation, result.Item1);
            Assert.Empty(result.Item2);
            Assert.NotNull(result.Item3);

            foreach (var phaseDetail in result.Item3.Keys)
            {
                Assert.Empty(result.Item3[phaseDetail]);
            }
        }

        /// <summary>
        /// Verifies that standard green, yellow, and red change interval boundaries are correctly detected as cycles.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailCyclesStep), "StandardCycles")]
        public async Task Process_StandardPhaseCycles_IdentifiesCorrectCycles()
        {
            var phaseNum = 2;
            var localLocation = CreateLocalMockLocation();
            AddLocalTestApproach(localLocation, phaseNum, false);

            var start = DateTime.Today.AddHours(8);
            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start, EventCode = (short)IndianaEnumerations.PhaseBeginGreen, EventParam = (short)phaseNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(30), EventCode = (short)IndianaEnumerations.PhaseBeginYellowChange, EventParam = (short)phaseNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(35), EventCode = (short)IndianaEnumerations.PhaseEndYellowChange, EventParam = (short)phaseNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(60), EventCode = (short)IndianaEnumerations.PhaseBeginGreen, EventParam = (short)phaseNum }
            };

            var sut = new CreateSplitFailCyclesStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events));

            var targetPhase = result.Item3.Keys.FirstOrDefault(p => p.PhaseNumber == phaseNum && !p.UseOverlap);
            Assert.NotNull(targetPhase);

            var cycles = result.Item3[targetPhase];
            Assert.Single(cycles);

            var cycle = cycles[0];
            Assert.Equal(start, cycle.GreenStart);
            Assert.Equal(start.AddSeconds(30), cycle.YellowStart);
            Assert.Equal(start.AddSeconds(35), cycle.RedStart);
            Assert.Equal(start.AddSeconds(60), cycle.GreenEnd);
        }

        /// <summary>
        /// Verifies that overlap-based cycles are correctly identified using overlap event codes.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailCyclesStep), "OverlapCycles")]
        public async Task Process_OverlapCycles_IdentifiesCorrectCycles()
        {
            var overlapNum = 1;
            var localLocation = CreateLocalMockLocation();
            AddLocalTestApproach(localLocation, overlapNum, true);

            var start = DateTime.Today.AddHours(8);
            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start, EventCode = (short)IndianaEnumerations.OverlapBeginGreen, EventParam = (short)overlapNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(40), EventCode = (short)IndianaEnumerations.OverlapBeginYellow, EventParam = (short)overlapNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(45), EventCode = (short)IndianaEnumerations.OverlapBeginRedClearance, EventParam = (short)overlapNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(80), EventCode = (short)IndianaEnumerations.OverlapBeginGreen, EventParam = (short)overlapNum }
            };

            var sut = new CreateSplitFailCyclesStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events));

            var targetPhase = result.Item3.Keys.FirstOrDefault(p => p.PhaseNumber == overlapNum && p.UseOverlap);
            Assert.NotNull(targetPhase);

            var cycles = result.Item3[targetPhase];
            Assert.Single(cycles);

            var cycle = cycles[0];
            Assert.Equal(start, cycle.GreenStart);
            Assert.Equal(start.AddSeconds(40), cycle.YellowStart);
            Assert.Equal(start.AddSeconds(45), cycle.RedStart);
            Assert.Equal(start.AddSeconds(80), cycle.GreenEnd);
        }

        /// <summary>
        /// Verifies that overlap cycles closing on a Dark overlap event are correctly parsed.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailCyclesStep), "OverlapDark")]
        public async Task Process_OverlapDarkClosure_ClosesOnDark()
        {
            var overlapNum = 1;
            var localLocation = CreateLocalMockLocation();
            AddLocalTestApproach(localLocation, overlapNum, true);

            var start = DateTime.Today.AddHours(8);
            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start, EventCode = (short)IndianaEnumerations.OverlapBeginGreen, EventParam = (short)overlapNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(40), EventCode = (short)IndianaEnumerations.OverlapBeginYellow, EventParam = (short)overlapNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(45), EventCode = (short)IndianaEnumerations.OverlapBeginRedClearance, EventParam = (short)overlapNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(80), EventCode = (short)IndianaEnumerations.OverlapDark, EventParam = (short)overlapNum }
            };

            var sut = new CreateSplitFailCyclesStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events));

            var targetPhase = result.Item3.Keys.FirstOrDefault(p => p.PhaseNumber == overlapNum && p.UseOverlap);
            Assert.NotNull(targetPhase);

            var cycles = result.Item3[targetPhase];
            Assert.Single(cycles);

            var cycle = cycles[0];
            Assert.Equal(start, cycle.GreenStart);
            Assert.Equal(start.AddSeconds(40), cycle.YellowStart);
            Assert.Equal(start.AddSeconds(45), cycle.RedStart);
            Assert.Equal(start.AddSeconds(80), cycle.GreenEnd);
        }

        /// <summary>
        /// Verifies that duplicate consecutive event logging does not crash the parser.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailCyclesStep), "DuplicateEvents")]
        public async Task Process_ConsecutiveDuplicates_HandlesGracefully()
        {
            var phaseNum = 2;
            var localLocation = CreateLocalMockLocation();
            AddLocalTestApproach(localLocation, phaseNum, false);

            var start = DateTime.Today.AddHours(8);
            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start, EventCode = (short)IndianaEnumerations.PhaseBeginGreen, EventParam = (short)phaseNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(30), EventCode = (short)IndianaEnumerations.PhaseBeginYellowChange, EventParam = (short)phaseNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(32), EventCode = (short)IndianaEnumerations.PhaseBeginYellowChange, EventParam = (short)phaseNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(35), EventCode = (short)IndianaEnumerations.PhaseEndYellowChange, EventParam = (short)phaseNum },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(60), EventCode = (short)IndianaEnumerations.PhaseBeginGreen, EventParam = (short)phaseNum }
            };

            var sut = new CreateSplitFailCyclesStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events));

            Assert.NotNull(result);
        }

        /// <summary>
        /// Verifies that event logging on Phase 2 is isolated from Phase 6.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailCyclesStep), "PhaseIsolation")]
        public async Task Process_PhaseIsolation_IsolatesEvents()
        {
            var phase2 = 2;
            var phase6 = 6;
            var localLocation = CreateLocalMockLocation();
            AddLocalTestApproach(localLocation, phase2, false);
            AddLocalTestApproach(localLocation, phase6, false);

            var start = DateTime.Today.AddHours(8);
            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start, EventCode = (short)IndianaEnumerations.PhaseBeginGreen, EventParam = (short)phase2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(30), EventCode = (short)IndianaEnumerations.PhaseBeginYellowChange, EventParam = (short)phase2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(35), EventCode = (short)IndianaEnumerations.PhaseEndYellowChange, EventParam = (short)phase2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(60), EventCode = (short)IndianaEnumerations.PhaseBeginGreen, EventParam = (short)phase2 }
            };

            var sut = new CreateSplitFailCyclesStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events));

            var targetPhase2 = result.Item3.Keys.FirstOrDefault(p => p.PhaseNumber == phase2 && !p.UseOverlap);
            var targetPhase6 = result.Item3.Keys.FirstOrDefault(p => p.PhaseNumber == phase6 && !p.UseOverlap);

            Assert.NotNull(targetPhase2);
            Assert.NotEmpty(result.Item3[targetPhase2]);

            if (targetPhase6 != null)
            {
                Assert.Empty(result.Item3[targetPhase6]);
            }
        }

        private Location CreateLocalMockLocation()
        {
            return new Location { LocationIdentifier = "MOCK_7115" };
        }

        private Approach AddLocalTestApproach(Location location, int phaseNum, bool useOverlap)
        {
            var approach = new Approach
            {
                Id = phaseNum,
                ProtectedPhaseNumber = phaseNum,
                IsProtectedPhaseOverlap = useOverlap,
                Mph = 35
            };
            location.Approaches.Add(approach);
            return approach;
        }
    }
}
