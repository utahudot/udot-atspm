using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Application.Analysis.PurdueCoordination;
using ATSPM.Application.Analysis.Workflows;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class CreatePreemptiveCyclesTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public CreatePreemptiveCyclesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Signal Grouping")]
        public async void CreatePreemptiveCyclesTestsSignalGrouping()
        {
            Assert.False(true);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Preempt Number Grouping")]
        public async void CreatePreemptiveCyclesTestsPreemptNumberGrouping()
        {
            Assert.False(true);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Start End Pass")]
        public async void CreatePreemptiveCyclesTestsStartEndPass()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First();
            var expected = new PreemptCycle()
            {
                Start = testData[0].Timestamp,
                StartInputOn = testData[0].Timestamp,
                EntryStarted = testData[1].Timestamp,
                BeginExitInterval = testData[3].Timestamp,
                End = testData[3].Timestamp,
                HasDelay = true
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Sort Order Pass")]
        public async void CreatePreemptiveCyclesTestsSortOrderPass()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First();
            var expected = new PreemptCycle()
            {
                Start = testData[1].Timestamp,
                StartInputOn = testData[1].Timestamp,
                EntryStarted = testData[0].Timestamp,
                BeginExitInterval = testData[2].Timestamp,
                End = testData[2].Timestamp,
                HasDelay = true
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Duplicate 102 Before 105 Event")]
        public async void CreatePreemptiveCyclesTestsDuplicate102Before105Event()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:30.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First();
            var expected = new PreemptCycle()
            {
                Start = testData[1].Timestamp,
                StartInputOn = testData[1].Timestamp,
                EntryStarted = testData[2].Timestamp,
                BeginExitInterval = testData[4].Timestamp,
                End = testData[4].Timestamp,
                HasDelay = true
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Duplicate 102 After 105 Event")]
        public async void CreatePreemptiveCyclesTestsDuplicate102After105Event()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:30.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First();
            var expected = new PreemptCycle()
            {
                Start = testData[0].Timestamp,
                StartInputOn = testData[0].Timestamp,
                EntryStarted = testData[1].Timestamp,
                BeginExitInterval = testData[4].Timestamp,
                End = testData[4].Timestamp,
                HasDelay = true
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Missing 102 Event")]
        public async void CreatePreemptiveCyclesTestsMissing102Event()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                //new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First();
            var expected = new PreemptCycle()
            {
                Start = testData[0].Timestamp,
                StartInputOn = null,
                EntryStarted = testData[0].Timestamp,
                BeginExitInterval = testData[2].Timestamp,
                End = testData[2].Timestamp,
                HasDelay = false
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Missing 105 Event")]
        public async void CreatePreemptiveCyclesTestsMissing105Event()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                //new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},
            };

            var result = await sut.ExecuteAsync(testData);

            var condition = result.Count;

            Assert.True(condition == 0);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Missing 104 Event")]
        public async void CreatePreemptiveCyclesTestsMissing104Event()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                //new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First();
            var expected = new PreemptCycle()
            {
                Start = testData[0].Timestamp,
                StartInputOn = testData[0].Timestamp,
                EntryStarted = testData[1].Timestamp,
                BeginExitInterval = testData[2].Timestamp,
                End = testData[2].Timestamp,
                HasDelay = true
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Missing 111 Event")]
        public async void CreatePreemptiveCyclesTestsMissing111Event()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                //new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First();
            var expected = new PreemptCycle()
            {
                Start = testData[0].Timestamp,
                StartInputOn = testData[0].Timestamp,
                EntryStarted = testData[1].Timestamp,
                BeginExitInterval = testData[2].Timestamp,
                End = testData[2].Timestamp,
                HasDelay = true
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "20 Minute Timout")]
        public async void CreatePreemptiveCyclesTests20MinuteTimeout()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:23:01.2"), EventCode = 111, EventParam = 1},
            };

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First();
            var expected = new PreemptCycle()
            {
                Start = testData[0].Timestamp,
                StartInputOn = testData[0].Timestamp,
                EntryStarted = testData[1].Timestamp,
                BeginExitInterval = null,
                End = testData[2].Timestamp,
                HasDelay = true
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Delay True")]
        public async void CreatePreemptiveCyclesTestsDelayTrue()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},
            };

            var result = await sut.ExecuteAsync(testData);

            var condition = result.First().HasDelay;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CreatePreemptiveCyclesTests), "Delay False")]
        public async void CreatePreemptiveCyclesTestsDelayFalse()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                //new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 111, EventParam = 1},
            };

            var result = await sut.ExecuteAsync(testData);

            var condition = result.First().HasDelay;

            Assert.False(condition);
        }

        [Fact]
        //[Trait(nameof(CreateRedToRedCycles), "Signal Grouping")]
        public async void TestTwo()
        {
            var sut = new PreemptiveStuff();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:01:01.1"), EventCode = 102, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:02:01.1"), EventCode = 105, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:03:01.1"), EventCode = 103, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:04:01.1"), EventCode = 106, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:05:01.1"), EventCode = 107, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:06:01.1"), EventCode = 110, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:07:01.1"), EventCode = 104, EventParam = 1},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 12:08:01.1"), EventCode = 111, EventParam = 1}
            };

            var result = await sut.ExecuteAsync(testData);

            foreach (var r in result)
            {
                _output.WriteLine($"r: {r}");
            }

            var actual = result.First();
            var expected = new PreemptCycle()
            {
                Start = testData[0].Timestamp,
                StartInputOn = testData[0].Timestamp,
                EntryStarted = testData[1].Timestamp,
                GateDown = testData[2].Timestamp,
                BeginTrackClearance = testData[3].Timestamp,
                BeginDwellService = testData[4].Timestamp,
                MaxPresenceExceeded = testData[5].Timestamp,
                BeginExitInterval = testData[7].Timestamp,
                End = testData[7].Timestamp,
                HasDelay = true
            };

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
