#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Analysis.WorkflowSteps/GroupSignalsByApproachesTests.cs
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
using ApplicationCoreTests.Fixtures;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class GroupLocationsByApproachesTests : IClassFixture<TestLocationFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Location _testLocation;

        public GroupLocationsByApproachesTests(ITestOutputHelper output, TestLocationFixture testLocation)
        {
            _output = output;
            _testLocation = testLocation.TestLocation;
        }

        /// <summary>
        /// Tests that it's cancelling correctly, should return a <see cref="TaskCanceledException"/>
        /// </summary>
        [Fact]
        [Trait(nameof(GroupLocationsByApproaches), "Cancellation")]
        public async void GroupLocationsByApproachesTestCancellation()
        {
            var source = new CancellationTokenSource();
            source.Cancel();

            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testLocation, testLogs);

            var sut = new GroupLocationsByApproaches();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExecuteAsync(testData, source.Token));
        }

        /// <summary>
        /// Tests the correct number of approaches are extracted from the test Location
        /// </summary>
        [Fact]
        [Trait(nameof(GroupLocationsByApproaches), "Approaches")]
        public async void GroupLocationsByApproachesTestApproaches()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testLocation, testLogs);

            var sut = new GroupLocationsByApproaches();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testLocation.Approaches.Select(s => Tuple.Create(s, testLogs));

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests that the correct sort order of the events has been applied
        /// </summary>
        [Fact]
        [Trait(nameof(GroupLocationsByApproaches), "Sort Order")]
        public async void GroupLocationsByApproachesTestSortOrder()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testLocation, testLogs);

            var sut = new GroupLocationsByApproaches();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testLocation.Approaches.Select(s => Tuple.Create(s, testLogs.OrderBy(o => o.Timestamp).AsEnumerable()));

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests that only events with a LocationIdentifier matching the test Location are forwarded
        /// </summary>
        [Fact]
        [Trait(nameof(GroupLocationsByApproaches), "Location")]
        public async void GroupLocationsByApproachesTestLocation()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 1, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 2, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 3, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 4, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 5, EventParam = 1},
            }.AsEnumerable();

            var testData = Tuple.Create(_testLocation, testLogs);

            var sut = new GroupLocationsByApproaches();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testLocation.Approaches.Select(s => Tuple.Create(s, testLogs.Where(w => w.SignalIdentifier == _testLocation.LocationIdentifier)));

            Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
