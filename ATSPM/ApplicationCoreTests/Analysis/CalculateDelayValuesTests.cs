using ATSPM.Application.Analysis;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis
{
    public class CalculateDelayValuesTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly List<RedToRedCycle> _redCycles;

        private StringWriter _consoleOut = new StringWriter();

        public CalculateDelayValuesTests(ITestOutputHelper output)
        {
            _output = output;

            _redCycles = new List<RedToRedCycle>
            {
                new RedToRedCycle()
                {
                    SignalId = "1001",
                    Phase = 1,
                    StartTime = DateTime.Parse("4/17/2023 8:00:0.1"),
                    GreenEvent = DateTime.Parse("4/17/2023 8:00:1.1"),
                    YellowEvent = DateTime.Parse("4/17/2023 8:00:2.1"),
                    EndTime = DateTime.Parse("4/17/2023 8:00:3.1")
                }
            };
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Compare Signal Pass")]
        public async void CalculateDelayValuesCompareSignalPassTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Compare Signal Fail")]
        public async void CalculateDelayValuesCompareSignalFailTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1002", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Compare Start Pass")]
        public async void CalculateDelayValuesCompareStartPassTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Compare Start Fail")]
        public async void CalculateDelayValuesCompareStartFailTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 7:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Compare End Pass")]
        public async void CalculateDelayValuesCompareEndPassTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Compare End Fail")]
        public async void CalculateDelayValuesCompareEndFailTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 9:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Data Check")]
        public async void CalculateDelayValuesDataCheckTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First();
            var expected = _redCycles.First();

            Assert.Equal(expected.SignalId, actual.SignalId);
            Assert.Equal(expected.Phase, actual.Phase);
            Assert.Equal(expected.StartTime, actual.StartTime);
            Assert.Equal(expected.EndTime, actual.EndTime);
            Assert.Equal(expected.GreenEvent, actual.GreenEvent);
            Assert.Equal(expected.YellowEvent, actual.YellowEvent);
        }

        /// <summary>
        /// Delay is only calculated for arrival on red
        /// </summary>
        [Fact]
        [Trait(nameof(CalculateDelayValues), "Red Delay")]
        public async void CalculateDelayValuesRedDelayTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().Delay;
            var expected = (_redCycles.First().GreenEvent - testEvents.First().TimeStamp).TotalSeconds;

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Delay is only calculated for arrival on red result should be 0
        /// </summary>
        [Fact]
        [Trait(nameof(CalculateDelayValues), "Green Delay")]
        public async void CalculateDelayValuesGreenDelayTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:1.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            _output.WriteLine($"{result.First().GreenEvent:yyyy-MM-dd'T'HH:mm:ss.f} - {result.First().TimeStamp:yyyy-MM-dd'T'HH:mm:ss.f}");

            var actual = result.First().Delay;
            var expected = 0;

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Delay is only calculated for arrival on red result should be 0
        /// </summary>
        [Fact]
        [Trait(nameof(CalculateDelayValues), "Yellow Delay")]
        public async void CalculateDelayValuesYellowDelayTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:2.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().Delay;
            var expected = 0;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Arrival on Red")]
        public async void CalculateDelayValuesArrivalOnRedTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.First().ArrivalType == ArrivalType.ArrivalOnRed;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Arrival on Green")]
        public async void CalculateDelayValuesArrivalOnGreenTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:1.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.First().ArrivalType == ArrivalType.ArrivalOnGreen;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Arrival on Yellow")]
        public async void CalculateDelayValuesArrivalOnYellowTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:2.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.First().ArrivalType == ArrivalType.ArrivalOnYellow;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDelayValues), "Null Cycles")]
        public async void CalculateDelayValuesNullCyclesTest()
        {
            var sut = new CalculateDelayValues();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
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
