using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class CreateRedToRedCyclesTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public CreateRedToRedCyclesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Timestamp Order")]
        public async void CreateRedToRedCyclesOrderTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2 },
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2 },
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2 },
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2 }
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First();
            var expected = new RedToRedCycle()
            {
                Start = DateTime.Parse("4/17/2023 8:01:48.8"),
                GreenEvent = DateTime.Parse("4/17/2023 8:03:11.7"),
                YellowEvent = DateTime.Parse("4/17/2023 8:04:13.7"),
                End = DateTime.Parse("4/17/2023 8:04:18.8")
            };

            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.GreenEvent, actual.GreenEvent);
            Assert.Equal(expected.YellowEvent, actual.YellowEvent);
            Assert.Equal(expected.End, actual.End);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Filter Events")]
        public async void CreateRedToRedCyclesFilterEventsTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:12.5"), EventCode = 101, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:14.2"), EventCode = 102, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2}
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First();
            var expected = new RedToRedCycle()
            {
                Start = DateTime.Parse("4/17/2023 8:01:48.8"),
                GreenEvent = DateTime.Parse("4/17/2023 8:03:11.7"),
                YellowEvent = DateTime.Parse("4/17/2023 8:04:13.7"),
                End = DateTime.Parse("4/17/2023 8:04:18.8")
            };

            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.GreenEvent, actual.GreenEvent);
            Assert.Equal(expected.YellowEvent, actual.YellowEvent);
            Assert.Equal(expected.End, actual.End);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "No Start Event")]
        public async void CreateRedToRedCyclesNoStartTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                //new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2}
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Count();
            var expected = 0;

            Assert.Equal(actual, expected);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "No End Event")]
        public async void CreateRedToRedCyclesNoEndTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2}
                //new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2}
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Count();
            var expected = 0;

            Assert.Equal(actual, expected);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "No Green Event")]
        public async void CreateRedToRedCyclesNoGreenTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                //new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2}
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Count();
            var expected = 0;

            Assert.Equal(actual, expected);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "No Yellow Event")]
        public async void CreateRedToRedCyclesNoYellowTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                //new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2}
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Count();
            var expected = 0;

            Assert.Equal(actual, expected);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Event Order")]
        public async void CreateRedToRedCyclesEventOrderTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2}
            };

            var result = await sut.ExecuteAsync(testData);
            var cycle = result.First();

            var condition = cycle.Start < cycle.GreenEvent && cycle.GreenEvent < cycle.YellowEvent && cycle.YellowEvent < cycle.End;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Mismatched Signals")]
        public async void CreateRedToRedCyclesMismatchedSignalTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1004", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2}
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Count();
            var expected = 0;

            Assert.Equal(actual, expected);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Mismatched Phases")]
        public async void CreateRedToRedCyclesMismatchedPhaseTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 1},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 3},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 4}
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Count();
            var expected = 0;

            Assert.Equal(actual, expected);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Signal Grouping")]
        public async void CreateRedToRedCyclesSignalGroupingTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 9:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 9:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 9:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 9:04:18.8"), EventCode = 9, EventParam = 2}
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Select(s => s.SignalIdentifier).Distinct();
            var expected = new List<string>()
            {
                "1001", "1002", "1003"
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Phase Grouping")]
        public async void CreateRedToRedCyclesPhaseGroupingTest()
        {
            var sut = new CreateRedToRedCycles();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 1},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 1},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 1},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 1},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 3},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 3},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 3},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 3},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:01:48.8"), EventCode = 9, EventParam = 3},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:03:11.7"), EventCode = 1, EventParam = 3},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:04:13.7"), EventCode = 8, EventParam = 3},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:04:18.8"), EventCode = 9, EventParam = 3}
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Select(s => s.PhaseNumber).Distinct();
            var expected = new List<int>()
            {
                1, 2, 3
            };

            Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
