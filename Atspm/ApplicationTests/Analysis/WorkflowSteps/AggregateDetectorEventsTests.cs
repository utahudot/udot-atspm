﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Analysis.WorkflowSteps/AggregateDetectorEventsTests.cs
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
    public class AggregateDetectorEventsTests : IClassFixture<TextDetectorFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Detector _testDetector;
        private readonly Location _testLocation;

        public AggregateDetectorEventsTests(ITestOutputHelper output, TextDetectorFixture testDetector)
        {
            _output = output;
            _testDetector = testDetector.TestDetector;
            _testLocation = _testDetector.Approach.Location;
        }

        [Fact]
        [Trait(nameof(AggregateDetectorEvents), "Cancellation")]
        public async void AggregateDetectorEventsTestsCancellation()
        {
            var source = new CancellationTokenSource();
            source.Cancel();

            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:01.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:02.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

            var sut = new AggregateDetectorEvents();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.ExecuteAsync(testData, source.Token));
        }

        /// <summary>
        /// Tests that only events with a LocationIdentifier matching the test Location are forwarded
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateDetectorEvents), "Location")]
        public async void AggregateDetectorEventsTestLocation()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:01.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:02.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:03:03.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 00:04:04.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

            var sut = new AggregateDetectorEvents();

            var actual = await sut.ExecuteAsync(testData);

            var expected = new DetectorEventCountAggregation()
            {
                LocationIdentifier = _testLocation.LocationIdentifier,
                ApproachId = _testDetector.ApproachId,
                DetectorPrimaryId = _testDetector.Id,
                EventCount = 2,
                Start = DateTime.Parse("4/17/2023 00:00:00.0"),
                End = DateTime.Parse("4/17/2023 00:15:00.0")
            };

            Assert.Collection(actual,
                a => Assert.Equivalent(expected, a));
        }

        /// <summary>
        /// Tests that only the event params that match the detector channel are forwarded
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateDetectorEvents), "Detector Filter")]
        public async void AggregateDetectorEventsTestDetectorFilter()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:01:01.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:02.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:03.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 100},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:04:04.5"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = 100},
            }.AsEnumerable();

            var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

            var sut = new AggregateDetectorEvents();

            var actual = await sut.ExecuteAsync(testData);

            var expected = new DetectorEventCountAggregation()
            {
                LocationIdentifier = _testLocation.LocationIdentifier,
                ApproachId = _testDetector.ApproachId,
                DetectorPrimaryId = _testDetector.Id,
                EventCount = 2,
                Start = DateTime.Parse("4/17/2023 00:00:00.0"),
                End = DateTime.Parse("4/17/2023 00:15:00.0")
            };

            Assert.Collection(actual,
                a => Assert.Equivalent(expected, a));
        }

        /// <summary>
        /// Tests that only IndianaEnumerations.VehicleDetectorOn events are forwarded
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateDetectorEvents), "Event Code Filter")]
        public async void AggregateDetectorEventsTestEventCodeFilter()
        {
            var testLogs = Enumerable.Range(1, 256).Select(s => new IndianaEvent()
            {
                LocationIdentifier = _testLocation.LocationIdentifier,
                Timestamp = DateTime.Parse("4/17/2023 00:00:01.0"),
                EventCode = Convert.ToInt16(s),
                EventParam = Convert.ToInt16(_testDetector.DetectorChannel)
            }).ToList().AsEnumerable();

            var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

            var sut = new AggregateDetectorEvents();

            var actual = await sut.ExecuteAsync(testData);

            var expected = new DetectorEventCountAggregation()
            {
                LocationIdentifier = _testLocation.LocationIdentifier,
                ApproachId = _testDetector.ApproachId,
                DetectorPrimaryId = _testDetector.Id,
                EventCount = 1,
                Start = DateTime.Parse("4/17/2023 00:00:00.0"),
                End = DateTime.Parse("4/17/2023 00:15:00.0")
            };

            Assert.Collection(actual,
                a => Assert.Equivalent(expected, a));
        }

        /// <summary>
        /// Tests if the timestamp of the event is equal to the start of the bin then it should be in that bin
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateDetectorEvents), "Bin Start")]
        public async void AggregateDetectorEventsTestBinStart()
        {
            var testLogs = new List<IndianaEvent>
            {
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:00:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:10:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

            var sut = new AggregateDetectorEvents();

            var actual = await sut.ExecuteAsync(testData);

            var expected = new DetectorEventCountAggregation()
            {
                LocationIdentifier = _testLocation.LocationIdentifier,
                ApproachId = _testDetector.ApproachId,
                DetectorPrimaryId = _testDetector.Id,
                EventCount = 3,
                Start = DateTime.Parse("4/17/2023 00:00:00.0"),
                End = DateTime.Parse("4/17/2023 00:15:00.0")
            };

            Assert.Collection(actual,
                a => Assert.Equivalent(expected, a));
        }

        /// <summary>
        /// Tests that the event timestamps are organized into the correct bin
        /// </summary>
        [Fact]
        [Trait(nameof(AggregateDetectorEvents), "Bin Groups")]
        public async void AggregateDetectorEventsTestBinGroups()
        {
            var testLogs = new List<IndianaEvent>
            {
                //group a
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:00:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:05:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:10:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},

                //group b
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:15:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:20:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
                new IndianaEvent() { LocationIdentifier = _testLocation.LocationIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:25:00.0"), EventCode = (int)IndianaEnumerations.VehicleDetectorOn, EventParam = Convert.ToInt16(_testDetector.DetectorChannel)},
            }.AsEnumerable();

            var testData = Tuple.Create(_testDetector, _testDetector.DetectorChannel, testLogs);

            var sut = new AggregateDetectorEvents();

            var actual = await sut.ExecuteAsync(testData);

            var expectedA = new DetectorEventCountAggregation()
            {
                LocationIdentifier = _testLocation.LocationIdentifier,
                ApproachId = _testDetector.ApproachId,
                DetectorPrimaryId = _testDetector.Id,
                EventCount = 3,
                Start = DateTime.Parse("4/17/2023 00:00:00.0"),
                End = DateTime.Parse("4/17/2023 00:15:00.0")
            };

            var expectedB = new DetectorEventCountAggregation()
            {
                LocationIdentifier = _testLocation.LocationIdentifier,
                ApproachId = _testDetector.ApproachId,
                DetectorPrimaryId = _testDetector.Id,
                EventCount = 3,
                Start = DateTime.Parse("4/17/2023 00:15:00.0"),
                End = DateTime.Parse("4/17/2023 00:30:00.0")
            };

            Assert.Collection(actual,
                a => Assert.Equivalent(expectedA, a),
                a => Assert.Equivalent(expectedB, a));
        }

        public void Dispose()
        {
        }
    }
}
