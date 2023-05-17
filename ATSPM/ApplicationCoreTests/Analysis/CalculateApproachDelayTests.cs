using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis
{
    public class CalculateApproachDelayTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly List<RedToRedCycle> _redCycles;

        public CalculateApproachDelayTests(ITestOutputHelper output)
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
        [Trait(nameof(CalculateApproachDelay), "Compare Signal Pass")]
        public async void CalculateApproachDelayCompareSignalPassTest()
        {
            var sut = new CalculateApproachDelay();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateApproachDelay), "Compare Signal Fail")]
        public async void CalculateApproachDelayCompareSignalFailTest()
        {
            var sut = new CalculateApproachDelay();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1002", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(CalculateApproachDelay), "Compare Start Pass")]
        public async void CalculateApproachDelayCompareStartPassTest()
        {
            var sut = new CalculateApproachDelay();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateApproachDelay), "Compare Start Fail")]
        public async void CalculateApproachDelayCompareStartFailTest()
        {
            var sut = new CalculateApproachDelay();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 7:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(CalculateApproachDelay), "Compare End Pass")]
        public async void CalculateApproachDelayCompareEndPassTest()
        {
            var sut = new CalculateApproachDelay();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateApproachDelay), "Compare End Fail")]
        public async void CalculateApproachDelayCompareEndFailTest()
        {
            var sut = new CalculateApproachDelay();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 9:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(CalculateApproachDelay), "Data Check")]
        public async void CalculateApproachDelayDataCheckTest()
        {
            var sut = new CalculateApproachDelay();

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
        [Trait(nameof(CalculateApproachDelay), "Red Delay")]
        public async void CalculateApproachDelayRedDelayTest()
        {
            var sut = new CalculateApproachDelay();

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
        [Trait(nameof(CalculateApproachDelay), "Green Delay")]
        public async void CalculateApproachDelayGreenDelayTest()
        {
            var sut = new CalculateApproachDelay();

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
        [Trait(nameof(CalculateApproachDelay), "Yellow Delay")]
        public async void CalculateApproachDelayYellowDelayTest()
        {
            var sut = new CalculateApproachDelay();

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
        [Trait(nameof(CalculateApproachDelay), "Arrival on Red")]
        public async void CalculateApproachDelayArrivalOnRedTest()
        {
            var sut = new CalculateApproachDelay();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:0.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.First().ArrivalType == ArrivalType.ArrivalOnRed;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateApproachDelay), "Arrival on Green")]
        public async void CalculateApproachDelayArrivalOnGreenTest()
        {
            var sut = new CalculateApproachDelay();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:1.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.First().ArrivalType == ArrivalType.ArrivalOnGreen;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateApproachDelay), "Arrival on Yellow")]
        public async void CalculateApproachDelayArrivalOnYellowTest()
        {
            var sut = new CalculateApproachDelay();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() {SignalId = "1001", DetChannel = 1, TimeStamp = DateTime.Parse("4/17/2023 8:00:2.5")}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.First().ArrivalType == ArrivalType.ArrivalOnYellow;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateApproachDelay), "Null Cycles")]
        public async void CalculateApproachDelayNullCyclesTest()
        {
            var sut = new CalculateApproachDelay();

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
