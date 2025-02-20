#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/GroupDetectorsByDetectorEventTests.cs
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
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    public class GroupDetectorsByDetectorEventTests : IClassFixture<TestApproachFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Approach _testApproach;
        private readonly Location _testLocation;

        public GroupDetectorsByDetectorEventTests(ITestOutputHelper output, TestApproachFixture testApproach)
        {
            _output = output;
            _testApproach = testApproach.TestApproach;
            _testLocation = _testApproach.Location;
        }

        /// <summary>
        /// Tests that it's cancelling correctly, should return a <see cref="TaskCanceledException"/>
        /// </summary>
        [Fact]
        [Trait(nameof(GroupDetectorsByDetectorEvent), "Cancellation")]
        public async void GroupDetectorsByDetectorEventTestCancellation()
        {
            var source = new CancellationTokenSource();
            source.Cancel();

            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:14.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 19},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 20},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:25.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 21},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:04:01.3"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 25},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 26},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:06:01.3"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 27},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 28},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new GroupDetectorsByDetectorEvent();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExecuteAsync(testData, source.Token));
        }

        /// <summary>
        /// Tests the correct number of detectors are extracted from the test approach
        /// and makes sure the event params are equals the detector channel
        /// </summary>
        [Fact]
        [Trait(nameof(GroupDetectorsByDetectorEvent), "Detectors")]
        public async void GroupDetectorsByDetectorEventTestDetectors()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:14.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 19},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 20},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:25.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 21},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:04:01.3"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 25},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 26},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:06:01.3"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 27},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 28},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new GroupDetectorsByDetectorEvent();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testApproach.Detectors.Select(s => Tuple.Create(s, s.DetectorChannel, testLogs.Where(w => w.EventParam == s.DetectorChannel)));

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests that the correct sort order of the events has been applied
        /// </summary>
        [Fact]
        [Trait(nameof(GroupDetectorsByDetectorEvent), "Sort Order")]
        public async void GroupDetectorsByDetectorEventTestSortOrder()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:06:01.3"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:14.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:25.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:04:01.3"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new GroupDetectorsByDetectorEvent();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testApproach.Detectors.Select(s => Tuple.Create(s, s.DetectorChannel, testLogs.OrderBy(o => o.Timestamp).AsEnumerable()));

            Assert.Equal(expected.Where(w => w.Item2 == 2), actual.Where(w => w.Item2 == 2));
        }

        /// <summary>
        /// Tests that only events with a LocationIdentifier matching the test Location are forwarded
        /// </summary>
        [Fact]
        [Trait(nameof(GroupDetectorsByDetectorEvent), "Location")]
        public async void GroupDetectorsByDetectorEventTestLocation()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:14.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 19},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 20},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:25.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 21},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:04:01.3"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 25},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 26},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:06:01.3"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 27},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 28},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:07:07.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 2},
            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new GroupDetectorsByDetectorEvent();

            var actual = await sut.ExecuteAsync(testData);

            var expected = _testApproach.Detectors.Select(s => Tuple.Create(s, s.DetectorChannel, testLogs
                .Where(w => w.LocationIdentifier == _testLocation.LocationIdentifier)
                .Where(w => w.EventParam == s.DetectorChannel)));

            Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
