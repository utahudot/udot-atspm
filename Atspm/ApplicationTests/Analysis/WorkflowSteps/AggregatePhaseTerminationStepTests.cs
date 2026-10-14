#region license
/// Copyright 2026 Utah Departement of Transportation
/// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/AggregatePhaseTerminationStepTests.cs
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
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    /// <summary>
    /// Unit tests for the AggregatePhaseTerminationStep workflow step.
    /// </summary>
    public class AggregatePhaseTerminationStepTests : WorkflowStepTestBase<AggregatePhaseTerminationStep, PhaseTerminationTestData, Location, Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PhaseTerminationAggregation>>
    {
        /// <summary>
        /// Initializes a new instance of the AggregatePhaseTerminationStepTests class.
        /// </summary>
        /// <param name="output">The xUnit test output helper.</param>
        /// <param name="testLocationFixture">The test location class fixture.</param>
        public AggregatePhaseTerminationStepTests(ITestOutputHelper output, TestLocationFixture testLocationFixture) : base(output, testLocationFixture) { }

        /// <inheritdoc/>
        protected override Location DefaultTestConfig => TestLocation;

        /// <inheritdoc/>
        protected override Tuple<Location, IEnumerable<IndianaEvent>> DefaultTestInput => Tuple.Create(TestLocation, (IEnumerable<IndianaEvent>)new List<IndianaEvent>());

        /// <inheritdoc/>
        protected override AggregatePhaseTerminationStep CreateStep(Location config, Tuple<Location, IEnumerable<IndianaEvent>> input, IEnumerable<PhaseTerminationAggregation> expected)
        {
            var timeline = DateTime.Today.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));
            return new AggregatePhaseTerminationStep(timeline);
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<PhaseTerminationAggregation>> ExecuteStepAsync(AggregatePhaseTerminationStep step, Location config, Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            return step.ExecuteAsync(input, cancelToken);
        }

        /// <summary>
        /// Verifies that an empty event stream does not crash the step and produces zero-filled binned rows.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePhaseTerminationStep), "EmptyEvents")]
        public async Task Process_EmptyEvents_ReturnsZeroFilledBins()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregatePhaseTerminationStep(timeline);

            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)new List<IndianaEvent>());
            var result = await sut.ExecuteAsync(input);

            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.All(list, a => Assert.Equal(0, a.GapOuts));
            Assert.All(list, a => Assert.Equal(0, a.MaxOuts));
            Assert.All(list, a => Assert.Equal(0, a.ForceOffs));
            Assert.All(list, a => Assert.Equal(0, a.Unknown));
            Assert.All(list, a => Assert.Equal(localLocation.LocationIdentifier, a.LocationIdentifier));
        }

        /// <summary>
        /// Verifies that 3 consecutive EC 4 events are binned and binned count is correct.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePhaseTerminationStep), "GapOuts")]
        public async Task Process_ConsecutiveGapOuts_AggregatesCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregatePhaseTerminationStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(5), EventCode = 4, EventParam = 2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(6), EventCode = 4, EventParam = 2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(7), EventCode = 4, EventParam = 2 }
            };

            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events);
            var result = await sut.ExecuteAsync(input);

            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var bin1 = list.FirstOrDefault(a => a.Start == start);
            var bin2 = list.FirstOrDefault(a => a.Start == start.AddMinutes(15));

            Assert.NotNull(bin1);
            Assert.Equal(1, bin1.GapOuts);
            Assert.Equal(0, bin1.MaxOuts);
            Assert.Equal(0, bin1.ForceOffs);
            Assert.Equal(0, bin1.Unknown);

            Assert.NotNull(bin2);
            Assert.Equal(0, bin2.GapOuts);
        }

        /// <summary>
        /// Verifies that 3 consecutive EC 5 events are binned and binned count is correct.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePhaseTerminationStep), "MaxOuts")]
        public async Task Process_ConsecutiveMaxOuts_AggregatesCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregatePhaseTerminationStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(5), EventCode = 5, EventParam = 2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(6), EventCode = 5, EventParam = 2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(7), EventCode = 5, EventParam = 2 }
            };

            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events);
            var result = await sut.ExecuteAsync(input);

            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var bin1 = list.FirstOrDefault(a => a.Start == start);
            var bin2 = list.FirstOrDefault(a => a.Start == start.AddMinutes(15));

            Assert.NotNull(bin1);
            Assert.Equal(0, bin1.GapOuts);
            Assert.Equal(1, bin1.MaxOuts);
            Assert.Equal(0, bin1.ForceOffs);
            Assert.Equal(0, bin1.Unknown);

            Assert.NotNull(bin2);
            Assert.Equal(0, bin2.MaxOuts);
        }

        /// <summary>
        /// Verifies that 3 consecutive EC 6 events are binned and binned count is correct.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePhaseTerminationStep), "ForceOffs")]
        public async Task Process_ConsecutiveForceOffs_AggregatesCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregatePhaseTerminationStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(5), EventCode = 6, EventParam = 2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(6), EventCode = 6, EventParam = 2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(7), EventCode = 6, EventParam = 2 }
            };

            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events);
            var result = await sut.ExecuteAsync(input);

            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var bin1 = list.FirstOrDefault(a => a.Start == start);
            var bin2 = list.FirstOrDefault(a => a.Start == start.AddMinutes(15));

            Assert.NotNull(bin1);
            Assert.Equal(0, bin1.GapOuts);
            Assert.Equal(0, bin1.MaxOuts);
            Assert.Equal(1, bin1.ForceOffs);
            Assert.Equal(0, bin1.Unknown);

            Assert.NotNull(bin2);
            Assert.Equal(0, bin2.ForceOffs);
        }

        /// <summary>
        /// Verifies that 2 consecutive EC 7 events are binned and binned count is correct.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePhaseTerminationStep), "UnknownTerminations")]
        public async Task Process_ConsecutiveGreenTerminations_AggregatesUnknownCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregatePhaseTerminationStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(5), EventCode = 7, EventParam = 2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(6), EventCode = 7, EventParam = 2 }
            };

            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events);
            var result = await sut.ExecuteAsync(input);

            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var bin1 = list.FirstOrDefault(a => a.Start == start);
            var bin2 = list.FirstOrDefault(a => a.Start == start.AddMinutes(15));

            Assert.NotNull(bin1);
            Assert.Equal(0, bin1.GapOuts);
            Assert.Equal(0, bin1.MaxOuts);
            Assert.Equal(0, bin1.ForceOffs);
            Assert.Equal(1, bin1.Unknown);

            Assert.NotNull(bin2);
            Assert.Equal(0, bin2.Unknown);
        }

        /// <summary>
        /// Verifies that events on other phases are ignored.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregatePhaseTerminationStep), "PhaseIsolation")]
        public async Task Process_PhaseIsolation_IgnoresOtherPhaseEvents()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregatePhaseTerminationStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(5), EventCode = 4, EventParam = 4 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(6), EventCode = 4, EventParam = 4 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(7), EventCode = 4, EventParam = 4 }
            };

            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events);
            var result = await sut.ExecuteAsync(input);

            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var bin1 = list.FirstOrDefault(a => a.Start == start);
            Assert.NotNull(bin1);
            Assert.Equal(0, bin1.GapOuts);
            Assert.Equal(0, bin1.MaxOuts);
            Assert.Equal(0, bin1.ForceOffs);
            Assert.Equal(0, bin1.Unknown);
        }

        private Location CreateLocalMockLocation()
        {
            var location = new Location { LocationIdentifier = "MOCK_7636" };
            location.Approaches.Add(new Approach { ProtectedPhaseNumber = 2 });
            return location;
        }
    }
}
