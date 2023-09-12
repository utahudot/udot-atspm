using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class AssignCyclesToVehiclesTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly List<RedToRedCycle> _redCycles;
        private readonly Detector _detector;

        public AssignCyclesToVehiclesTests(ITestOutputHelper output)
        {
            _output = output;

            _redCycles = new List<RedToRedCycle>
            {
                new RedToRedCycle()
                {
                    SignalIdentifier = "1001",
                    PhaseNumber = 1,
                    Start = DateTime.Parse("4/17/2023 8:00:0.1"),
                    GreenEvent = DateTime.Parse("4/17/2023 8:00:1.1"),
                    YellowEvent = DateTime.Parse("4/17/2023 8:00:2.1"),
                    End = DateTime.Parse("4/17/2023 8:00:3.1")
                }
            };

            _detector = new Detector()
            {
                DetChannel = 1,
                DistanceFromStopBar = 340,
                LatencyCorrection = 1.2,
                Approach = new Approach()
                {
                    ProtectedPhaseNumber = 2,
                    DirectionTypeId = DirectionTypes.NB,
                    Mph = 45,
                    Signal = new Signal()
                    {
                        SignalIdentifier = "1001"
                    }
                }
            };
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Compare Signal Pass")]
        public async void AssignCyclesToVehiclesCompareSignalPassTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Compare Signal Fail")]
        public async void AssignCyclesToVehiclesCompareSignalFailTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            _redCycles.First().SignalIdentifier = "1002";

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Compare Start Pass")]
        public async void AssignCyclesToVehiclesCompareStartPassTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Compare Start Fail")]
        public async void AssignCyclesToVehiclesCompareStartFailTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 7:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Compare End Pass")]
        public async void AssignCyclesToVehiclesCompareEndPassTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Compare End Fail")]
        public async void AssignCyclesToVehiclesCompareEndFailTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 9:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Data Check")]
        public async void AssignCyclesToVehiclesDataCheckTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First();
            var expected = _redCycles.First();

            Assert.Equal(expected.SignalIdentifier, actual.SignalIdentifier);
            Assert.Equal(expected.PhaseNumber, actual.PhaseNumber);
            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.End, actual.End);
            Assert.Equal(expected.GreenEvent, actual.GreenEvent);
            Assert.Equal(expected.YellowEvent, actual.YellowEvent);
        }

        /// <summary>
        /// Delay is only calculated for arrival on red
        /// </summary>
        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Red Delay")]
        public async void AssignCyclesToVehiclesRedDelayTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().Delay;
            var expected = (_redCycles.First().GreenEvent - testEvents.First().CorrectedTimeStamp).TotalSeconds;

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Delay is only calculated for arrival on red result should be 0
        /// </summary>
        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Green Delay")]
        public async void AssignCyclesToVehiclesGreenDelayTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            _output.WriteLine($"{result.First().GreenEvent:yyyy-MM-dd'T'HH:mm:ss.f} - {result.First().CorrectedTimeStamp:yyyy-MM-dd'T'HH:mm:ss.f}");

            var actual = result.First().Delay;
            var expected = 0;

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Delay is only calculated for arrival on red result should be 0
        /// </summary>
        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Yellow Delay")]
        public async void AssignCyclesToVehiclesYellowDelayTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().Delay;
            var expected = 0;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Arrival on Red")]
        public async void AssignCyclesToVehiclesArrivalOnRedTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.First().ArrivalType == ArrivalType.ArrivalOnRed;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Arrival on Green")]
        public async void AssignCyclesToVehiclesArrivalOnGreenTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.First().ArrivalType == ArrivalType.ArrivalOnGreen;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Arrival on Yellow")]
        public async void AssignCyclesToVehiclesArrivalOnYellowTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.First().ArrivalType == ArrivalType.ArrivalOnYellow;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(AssignCyclesToVehicles), "Null Cycles")]
        public async void AssignCyclesToVehiclesNullCyclesTest()
        {
            var sut = new AssignCyclesToVehicles();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, null));

            var condition = result.Count() == 0;

            Assert.True(condition);
        }

        public void Dispose()
        {
        }
    }
}
