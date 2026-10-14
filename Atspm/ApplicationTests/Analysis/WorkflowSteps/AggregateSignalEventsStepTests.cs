#region license
/// Copyright 2026 Utah Departement of Transportation
/// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/AggregateSignalEventsStepTests.cs
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
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    /// <summary>
    /// Unit tests for the AggregateSignalEventsStep workflow step.
    /// </summary>
    public class AggregateSignalEventsStepTests : WorkflowStepTestBase<AggregateSignalEventsStep, AggregateSignalEventCountTestData, Location, Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<SignalEventCountAggregation>>
    {
        /// <summary>
        /// Initializes a new instance of the AggregateSignalEventsStepTests class.
        /// </summary>
        /// <param name="output">The xUnit test output helper.</param>
        /// <param name="testLocationFixture">The test location class fixture.</param>
        public AggregateSignalEventsStepTests(ITestOutputHelper output, TestLocationFixture testLocationFixture) : base(output, testLocationFixture) { }

        /// <inheritdoc/>
        protected override Location DefaultTestConfig => TestLocation;

        /// <inheritdoc/>
        protected override Tuple<Location, IEnumerable<IndianaEvent>> DefaultTestInput => Tuple.Create(TestLocation, (IEnumerable<IndianaEvent>)new List<IndianaEvent>());

        /// <inheritdoc/>
        protected override AggregateSignalEventsStep CreateStep(Location config, Tuple<Location, IEnumerable<IndianaEvent>> input, IEnumerable<SignalEventCountAggregation> expected)
        {
            var timeline = DateTime.Today.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));
            return new AggregateSignalEventsStep(timeline);
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<SignalEventCountAggregation>> ExecuteStepAsync(AggregateSignalEventsStep step, Location config, Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            return step.ExecuteAsync(input, cancelToken);
        }

        /// <summary>
        /// Verifies that an empty event stream does not crash the step and produces zero-filled binned rows.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateSignalEventsStep), "EmptyEvents")]
        public async Task Process_EmptyEvents_ReturnsZeroFilledBins()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregateSignalEventsStep(timeline);

            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)new List<IndianaEvent>());
            var result = await sut.ExecuteAsync(input);

            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.All(list, a => Assert.Equal(0, a.EventCount));
            Assert.All(list, a => Assert.Equal(localLocation.LocationIdentifier, a.LocationIdentifier));
        }

        /// <summary>
        /// Verifies that events belonging to the signal are counted and assigned to the correct timeline bin.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateSignalEventsStep), "EventCounting")]
        public async Task Process_StandardEvents_CountsAndBinsCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(30), TimeSpan.FromMinutes(15));
            var sut = new AggregateSignalEventsStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(5), EventCode = 1, EventParam = 1 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(10), EventCode = 1, EventParam = 2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(20), EventCode = 1, EventParam = 1 }
            };

            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events);
            var result = await sut.ExecuteAsync(input);

            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var bin1 = list.FirstOrDefault(a => a.Start == start);
            var bin2 = list.FirstOrDefault(a => a.Start == start.AddMinutes(15));

            Assert.NotNull(bin1);
            Assert.Equal(2, bin1.EventCount);

            Assert.NotNull(bin2);
            Assert.Equal(1, bin2.EventCount);
        }

        /// <summary>
        /// Verifies that events occurring before the timeline's first segment start or after the last segment end are ignored.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateSignalEventsStep), "BoundaryExclusion")]
        public async Task Process_BoundaryEvents_ExcludesCorrectly()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(15), TimeSpan.FromMinutes(15));
            var sut = new AggregateSignalEventsStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(-5), EventCode = 1, EventParam = 1 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddMinutes(20), EventCode = 1, EventParam = 1 }
            };

            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events);
            var result = await sut.ExecuteAsync(input);

            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal(0, list[0].EventCount);
        }

        /// <summary>
        /// Verifies that events matching a different location identifier are ignored by the log specification filter.
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateSignalEventsStep), "LocationIsolation")]
        public async Task Process_DifferentLocationEvents_IgnoresThem()
        {
            var localLocation = CreateLocalMockLocation();
            var start = DateTime.Today.AddHours(8);
            var timeline = new Timeline<StartEndRange>(start, start.AddMinutes(15), TimeSpan.FromMinutes(15));
            var sut = new AggregateSignalEventsStep(timeline);

            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = "OTHER_SIGNAL", Timestamp = start.AddMinutes(5), EventCode = 1, EventParam = 1 }
            };

            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events);
            var result = await sut.ExecuteAsync(input);

            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal(0, list[0].EventCount);
        }

        private Location CreateLocalMockLocation()
        {
            return new Location { LocationIdentifier = "MOCK_8899" };
        }
    }
}
