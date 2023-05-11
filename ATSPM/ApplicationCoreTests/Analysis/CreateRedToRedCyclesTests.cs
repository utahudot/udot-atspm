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
    public class CreateRedToRedCyclesTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private StringWriter _consoleOut = new StringWriter();

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

            var actual = result.First().ToList().First();
            var expected = new RedToRedCycle()
            {
                StartTime = DateTime.Parse("4/17/2023 8:01:48.8"),
                GreenEvent = DateTime.Parse("4/17/2023 8:03:11.7"),
                YellowEvent = DateTime.Parse("4/17/2023 8:04:13.7"),
                EndTime = DateTime.Parse("4/17/2023 8:04:18.8")
            };

            Assert.Equal(expected.StartTime, actual.StartTime);
            Assert.Equal(expected.GreenEvent, actual.GreenEvent);
            Assert.Equal(expected.YellowEvent, actual.YellowEvent);
            Assert.Equal(expected.EndTime, actual.EndTime);
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

            var actual = result.First().ToList().First();
            var expected = new RedToRedCycle()
            {
                StartTime = DateTime.Parse("4/17/2023 8:01:48.8"),
                GreenEvent = DateTime.Parse("4/17/2023 8:03:11.7"),
                YellowEvent = DateTime.Parse("4/17/2023 8:04:13.7"),
                EndTime = DateTime.Parse("4/17/2023 8:04:18.8")
            };

            Assert.Equal(expected.StartTime, actual.StartTime);
            Assert.Equal(expected.GreenEvent, actual.GreenEvent);
            Assert.Equal(expected.YellowEvent, actual.YellowEvent);
            Assert.Equal(expected.EndTime, actual.EndTime);
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

            Assert.Collection(result, a => { });
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

            Assert.Collection(result, a => { });
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

            Assert.Collection(result, a => { });
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

            Assert.Collection(result, a => { });
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
            var expected = 4;

            Assert.Equal(expected, actual);
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
            var expected = 4;

            Assert.Equal(expected, actual);
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

            var actual = result.SelectMany(s => s.Select(s => s.SignalId).Distinct());
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

            var actual = result.SelectMany(s => s.Select(s => s.Phase).Distinct());
            var expected = new List<int>()
            {
                1, 2, 3
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreateRedToRedCycles), "Signal/Phase Grouping")]
        public async void CreateRedToRedCyclesSignalPhaseGroupingTest()
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

                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 1},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 1},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 1},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 1},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 3},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 3},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 3},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 3}
            };

            var result = await sut.ExecuteAsync(testData);


            foreach (var r in result.ToList())
            {
                _output.WriteLine($"{r.ToList()[0]}");
            }

            var actual = result.SelectMany(s => s.Select(s => s.Phase).Distinct()).Distinct();
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
