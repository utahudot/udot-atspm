#region license
/// Copyright 2026 Utah Departement of Transportation
/// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/CreateSplitFailDetectorActivationsStepTests.cs
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
    /// Unit tests for the CreateSplitFailDetectorActivationsStep workflow step.
    /// </summary>
    public class CreateSplitFailDetectorActivationsStepTests : WorkflowStepTestBase<CreateSplitFailDetectorActivationsStep, CreateSplitFailDetectorActivationsTestData, Location, Tuple<IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>>, Tuple<Location, Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>>>
    {
        /// <summary>
        /// Initializes a new instance of the CreateSplitFailDetectorActivationsStepTests class.
        /// </summary>
        /// <param name="output">The xUnit test output helper.</param>
        /// <param name="testLocationFixture">The test location class fixture.</param>
        public CreateSplitFailDetectorActivationsStepTests(ITestOutputHelper output, TestLocationFixture testLocationFixture) : base(output, testLocationFixture) { }

        /// <inheritdoc/>
        protected override Location DefaultTestConfig => TestLocation;

        /// <inheritdoc/>
        protected override Tuple<IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>> DefaultTestInput => Tuple.Create((IEnumerable<IndianaEvent>)new List<IndianaEvent>(), new Dictionary<PhaseDetail, List<CycleTimestamps>>());

        /// <inheritdoc/>
        protected override CreateSplitFailDetectorActivationsStep CreateStep(Location config, Tuple<IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>> input, Tuple<Location, Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>> expected)
        {
            return new CreateSplitFailDetectorActivationsStep();
        }

        /// <inheritdoc/>
        protected override Task<Tuple<Location, Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>>> ExecuteStepAsync(CreateSplitFailDetectorActivationsStep step, Location config, Tuple<IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>> input, CancellationToken cancelToken = default)
        {
            return step.ExecuteAsync(Tuple.Create(config, input.Item1, input.Item2), cancelToken);
        }

        /// <summary>
        /// Verifies that an approach with no stopbar presence (SBP) detectors returns empty presence intervals list.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailDetectorActivationsStep), "NoStopbarDetectors")]
        public async Task Process_NoStopbarDetectors_ReturnsEmptyPresence()
        {
            var phaseNum = 8;
            var localLocation = CreateLocalMockLocation();
            var approach = AddLocalTestApproach(localLocation, phaseNum);

            var phaseDetail = new PhaseDetail { Approach = approach, UseOverlap = false, PhaseNumber = approach.ProtectedPhaseNumber };
            var cycles = new Dictionary<PhaseDetail, List<CycleTimestamps>>
            {
                { phaseDetail, new List<CycleTimestamps>() }
            };

            var sut = new CreateSplitFailDetectorActivationsStep();
            var input = Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)new List<IndianaEvent>(), cycles);

            var result = await sut.ExecuteAsync(input);

            var outputPresence = result.Item3[phaseDetail];
            Assert.Empty(outputPresence);
        }

        /// <summary>
        /// Verifies that detector ON and OFF event transitions are accurately mapped to continuous active presence intervals.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailDetectorActivationsStep), "SingleDetectorReconstruction")]
        public async Task Process_SingleDetector_ReconstructsPresenceIntervals()
        {
            var phaseNum = 2;
            var detectorChannel = 12;
            var localLocation = CreateLocalMockLocation();
            var approach = AddLocalTestApproach(localLocation, phaseNum);
            AddLocalStopBarDetector(approach, detectorChannel, 0, 0);

            var start = DateTime.Today.AddHours(8);
            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start, EventCode = (short)IndianaEnumerations.VehicleDetectorOn, EventParam = (short)detectorChannel },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(15), EventCode = (short)IndianaEnumerations.VehicleDetectorOff, EventParam = (short)detectorChannel }
            };

            var phaseDetail = new PhaseDetail { Approach = approach, UseOverlap = false, PhaseNumber = approach.ProtectedPhaseNumber };
            var cycles = new Dictionary<PhaseDetail, List<CycleTimestamps>>
            {
                { phaseDetail, new List<CycleTimestamps>() }
            };

            var sut = new CreateSplitFailDetectorActivationsStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events, cycles));

            var presence = result.Item3[phaseDetail];
            Assert.Single(presence);

            var interval = presence[0];
            Assert.Equal(start, interval.Item1);
            Assert.Equal(start.AddSeconds(15), interval.Item2);
        }

        /// <summary>
        /// Verifies that distance-based travel time and system latency corrections are correctly applied.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailDetectorActivationsStep), "TravelTimeLatency")]
        public async Task Process_TravelTimeAndLatency_AdjustsTimestamps()
        {
            var phaseNum = 2;
            var detectorChannel = 12;
            var localLocation = CreateLocalMockLocation();
            var approach = AddLocalTestApproach(localLocation, phaseNum, 30);

            var distance = 150;
            var latency = 1.0;
            AddLocalStopBarDetector(approach, detectorChannel, distance, latency);

            var start = DateTime.Today.AddHours(8);
            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start, EventCode = (short)IndianaEnumerations.VehicleDetectorOn, EventParam = (short)detectorChannel },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(15), EventCode = (short)IndianaEnumerations.VehicleDetectorOff, EventParam = (short)detectorChannel }
            };

            var phaseDetail = new PhaseDetail { Approach = approach, UseOverlap = false, PhaseNumber = approach.ProtectedPhaseNumber };
            var cycles = new Dictionary<PhaseDetail, List<CycleTimestamps>>
            {
                { phaseDetail, new List<CycleTimestamps>() }
            };

            var sut = new CreateSplitFailDetectorActivationsStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events, cycles));

            var presence = result.Item3[phaseDetail];
            Assert.Single(presence);

            var expectedOn = AtspmMath.AdjustTimeStamp(start, approach.Mph ?? 0, distance, latency);
            var expectedOff = AtspmMath.AdjustTimeStamp(start.AddSeconds(15), approach.Mph ?? 0, distance, latency);

            var interval = presence[0];
            Assert.Equal(expectedOn, interval.Item1);
            Assert.Equal(expectedOff, interval.Item2);
        }

        /// <summary>
        /// Verifies that overlapping active presence intervals across parallel lanes are merged into clean, approach-wide intervals.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailDetectorActivationsStep), "ParallelLaneMerging")]
        public async Task Process_ParallelLanes_MergesOverlappingPresence()
        {
            var phaseNum = 2;
            var localLocation = CreateLocalMockLocation();
            var approach = AddLocalTestApproach(localLocation, phaseNum);

            var ch1 = 12;
            var ch2 = 13;
            AddLocalStopBarDetector(approach, ch1, 0, 0);
            AddLocalStopBarDetector(approach, ch2, 0, 0);

            var start = DateTime.Today.AddHours(8);
            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start, EventCode = (short)IndianaEnumerations.VehicleDetectorOn, EventParam = (short)ch1 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(10), EventCode = (short)IndianaEnumerations.VehicleDetectorOff, EventParam = (short)ch1 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(5), EventCode = (short)IndianaEnumerations.VehicleDetectorOn, EventParam = (short)ch2 },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(15), EventCode = (short)IndianaEnumerations.VehicleDetectorOff, EventParam = (short)ch2 }
            };

            var phaseDetail = new PhaseDetail { Approach = approach, UseOverlap = false, PhaseNumber = approach.ProtectedPhaseNumber };
            var cycles = new Dictionary<PhaseDetail, List<CycleTimestamps>>
            {
                { phaseDetail, new List<CycleTimestamps>() }
            };

            var sut = new CreateSplitFailDetectorActivationsStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events, cycles));

            var presence = result.Item3[phaseDetail];
            Assert.Single(presence);

            var interval = presence[0];
            Assert.Equal(start, interval.Item1);
            Assert.Equal(start.AddSeconds(15), interval.Item2);
        }

        /// <summary>
        /// Verifies that standard operational approach speed executes successfully.
        /// </summary>
        [Fact]
        [Trait(nameof(CreateSplitFailDetectorActivationsStep), "OperationalSpeed")]
        public async Task Process_ValidSpeed_ExecutesSuccessfully()
        {
            var phaseNum = 2;
            var detectorChannel = 12;
            var localLocation = CreateLocalMockLocation();
            var approach = AddLocalTestApproach(localLocation, phaseNum, 35);
            AddLocalStopBarDetector(approach, detectorChannel, 150, 1.0);

            var start = DateTime.Today.AddHours(8);
            var events = new List<IndianaEvent>
            {
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start, EventCode = (short)IndianaEnumerations.VehicleDetectorOn, EventParam = (short)detectorChannel },
                new() { LocationIdentifier = localLocation.LocationIdentifier, Timestamp = start.AddSeconds(15), EventCode = (short)IndianaEnumerations.VehicleDetectorOff, EventParam = (short)detectorChannel }
            };

            var phaseDetail = new PhaseDetail { Approach = approach, UseOverlap = false, PhaseNumber = approach.ProtectedPhaseNumber };
            var cycles = new Dictionary<PhaseDetail, List<CycleTimestamps>>
            {
                { phaseDetail, new List<CycleTimestamps>() }
            };

            var sut = new CreateSplitFailDetectorActivationsStep();
            var result = await sut.ExecuteAsync(Tuple.Create(localLocation, (IEnumerable<IndianaEvent>)events, cycles));

            Assert.NotNull(result);
        }

        private Location CreateLocalMockLocation()
        {
            return new Location { LocationIdentifier = "MOCK_7115" };
        }

        private Approach AddLocalTestApproach(Location location, int phaseNum, int speed = 35)
        {
            var approach = new Approach
            {
                Id = phaseNum,
                ProtectedPhaseNumber = phaseNum,
                Mph = speed
            };
            location.Approaches.Add(approach);
            return approach;
        }

        private Detector AddLocalStopBarDetector(Approach approach, int channel, int distance = 0, double latency = 0.0)
        {
            var detector = new Detector
            {
                Id = channel,
                DetectorChannel = channel,
                DistanceFromStopBar = distance,
                LatencyCorrection = latency
            };
            detector.DetectionTypes.Add(new DetectionType { Id = DetectionTypes.SBP, Description = "Stop Bar Presence" });
            approach.Detectors.Add(detector);
            return detector;
        }
    }
}
