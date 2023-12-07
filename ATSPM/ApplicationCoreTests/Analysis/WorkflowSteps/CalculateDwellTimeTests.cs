using ApplicationCoreTests.Analysis.TestObjects;
using ApplicationCoreTests.Fixtures;
using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class CalculateDwellTimeTests : IClassFixture<TestSignalFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Signal _testSignal;
        private const int param = 1;

        public CalculateDwellTimeTests(ITestOutputHelper output, TestSignalFixture testSignal)
        {
            _output = output;
            _testSignal = testSignal.TestSignal;
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Data Check")]
        public async void CalculateDwellTimeTestsDataCheck()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testSignal, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var expected = new DwellTimeValue()
            {
                SignalIdentifier = _testSignal.SignalIdentifier,
                PreemptNumber = param,
                Seconds = TimeSpan.FromSeconds(42),
                Start = DateTime.Parse("2023-04-17T00:02:25.5"),
                End = DateTime.Parse("2023-04-17T00:03:07.5")
            };

            Assert.Equivalent(_testSignal, result.Item1);
            Assert.Equivalent(expected, result.Item2.First());
            Assert.Equivalent(param, result.Item3);
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Sort Order")]
        public async void CalculateDwellTimeTestsSortOrder()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testSignal, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var expected = new DwellTimeValue()
            {
                SignalIdentifier = _testSignal.SignalIdentifier,
                PreemptNumber = param,
                Seconds = TimeSpan.FromSeconds(42),
                Start = DateTime.Parse("2023-04-17T00:02:25.5"),
                End = DateTime.Parse("2023-04-17T00:03:07.5")
            };

            Assert.Equivalent(_testSignal, result.Item1);
            Assert.Equivalent(expected, result.Item2.First());
            Assert.Equivalent(param, result.Item3);
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Preempt Number Filter")]
        public async void CalculateDwellTimeTestsPreemptNumberFilter()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 01:02:14.5"), EventCode = 102, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 01:02:20.5"), EventCode = 105, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 01:02:25.5"), EventCode = 107, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 01:03:01.3"), EventCode = 104, EventParam = 2},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 01:03:07.5"), EventCode = 111, EventParam = 2},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testSignal, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var condition = result.Item2.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Signal Filter")]
        public async void CalculateDwellTimeTestsSignalFilter()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 10:02:14.5"), EventCode = 102, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 10:02:20.5"), EventCode = 105, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 10:02:25.5"), EventCode = 107, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 10:03:01.3"), EventCode = 104, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 10:03:07.5"), EventCode = 111, EventParam = param},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testSignal, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var condition = result.Item2.Count() == 1;

            Assert.True(condition);
        }

        public void Dispose()
        {
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Missing 107 Event")]
        public async void CalculateDwellTimeTestsMissing107Event()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                //new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testSignal, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var condition = result.Item2?.Count() == 0;

            _output.WriteLine($"condition: {condition}");

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateDwellTime), "Missing 111 Event")]
        public async void CalculateDwellTimeTestsMissing111Event()
        {
            var testLogs = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:14.5"), EventCode = 102, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:20.5"), EventCode = 105, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:02:25.5"), EventCode = 107, EventParam = param},
                new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:01.3"), EventCode = 104, EventParam = param},
                //new ControllerEventLog() { SignalIdentifier = _testSignal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 00:03:07.5"), EventCode = 111, EventParam = param},
            }.AsEnumerable();

            var sut = new CalculateDwellTime();

            var testData = Tuple.Create(_testSignal, testLogs, param);

            var result = await sut.ExecuteAsync(testData);

            var condition = result.Item2?.Count() == 0;

            _output.WriteLine($"condition: {condition}");

            Assert.True(condition);
        }

        [Theory]
        [InlineData(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculateDwellTimeTestData1.json")]
        [Trait(nameof(CalculateDwellTime), "From File")]
        public async void CalculateDwellTimeFromFileTest(string file)
        {
            var json = File.ReadAllText(new FileInfo(file).FullName);
            var testFile = JsonConvert.DeserializeObject<PreemptiveProcessTestData>(json, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            _output.WriteLine($"Configuration: {testFile.Configuration}");
            _output.WriteLine($"Input: {testFile.Input.Count}");
            _output.WriteLine($"Output: {testFile.Output.Count}");

            var testData = Tuple.Create<Signal, IEnumerable<ControllerEventLog>, int>(testFile.Configuration, testFile.Input, 1);

            var sut = new CalculateDwellTime();

            var result = await sut.ExecuteAsync(testData);

            var expected = testFile.Output;
            var actual = result.Item2;

            Assert.Equivalent(expected, actual);
        }
    }
}
