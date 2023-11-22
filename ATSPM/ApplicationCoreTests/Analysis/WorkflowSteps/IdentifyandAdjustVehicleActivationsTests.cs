using ApplicationCoreTests.Analysis.TestObjects;
using ApplicationCoreTests.Fixtures;
using ATSPM.Application;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class IdentifyandAdjustVehicleActivationsTests : IClassFixture<TestApproachFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Approach _testApproach;

        public IdentifyandAdjustVehicleActivationsTests(ITestOutputHelper output, TestApproachFixture testApproach)
        {
            _output = output;
            _testApproach = testApproach.TestApproach;
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Signal Filter")]
        public async void IdentifyandAdjustVehicleActivationsSignalFilterTest()
        {
            var correct = Enumerable.Range(1, 5).Select(s => new ControllerEventLog()
            {
                SignalIdentifier = _testApproach.Signal.SignalIdentifier,
                Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                EventCode = (int)DataLoggerEnum.DetectorOn,
                EventParam = 2
            });

            var incorrect = Enumerable.Range(1, 5).Select(s => new ControllerEventLog()
            {
                SignalIdentifier = "1001",
                Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                EventCode = (int)DataLoggerEnum.DetectorOn,
                EventParam = 2
            });

            var testLogs = correct.Union(incorrect);

            foreach (var l in testLogs)
            {
                _output.WriteLine($"logs: {l}");
            }

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new IdentifyandAdjustVehicleActivations();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"corrected event: {l}");
            }

            var expected = correct.Select(s => s.SignalIdentifier).Distinct().OrderBy(o => o);
            var actual = result.Item2.Select(s => s.SignalIdentifier).Distinct().OrderBy(o => o);

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Detector Filter")]
        public async void IdentifyandAdjustVehicleActivationsDetectorFilterTest()
        {
            var correct = Enumerable.Range(1, 5).Select(s => new ControllerEventLog()
            {
                SignalIdentifier = _testApproach.Signal.SignalIdentifier,
                Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                EventCode = (int)DataLoggerEnum.DetectorOn,
                EventParam = 2
            });

            var incorrect = Enumerable.Range(1, 5).Select(s => new ControllerEventLog()
            {
                SignalIdentifier = _testApproach.Signal.SignalIdentifier,
                Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                EventCode = (int)DataLoggerEnum.DetectorOn,
                EventParam = 100
            });

            var testLogs = correct.Union(incorrect);

            foreach (var l in testLogs)
            {
                _output.WriteLine($"logs: {l}");
            }

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new IdentifyandAdjustVehicleActivations();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"corrected event: {l}");
            }

            var expected = correct.Select(s => s.EventParam).Distinct().OrderBy(o => o);
            var actual = result.Item2.Select(s => s.DetectorChannel).Distinct().OrderBy(o => o);

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Code Filter")]
        public async void IdentifyandAdjustVehicleActivationsCodeFilterTest()
        {
            var correct = Enumerable.Range(1, 5).Select(s => new ControllerEventLog()
            {
                SignalIdentifier = _testApproach.Signal.SignalIdentifier,
                Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                EventCode = (int)DataLoggerEnum.DetectorOn,
                EventParam = 2
            });

            var incorrect = Enumerable.Range(1, 5).Select(s => new ControllerEventLog()
            {
                SignalIdentifier = _testApproach.Signal.SignalIdentifier,
                Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                EventCode = Random.Shared.Next(1, 50),
                EventParam = 2
            });

            var testLogs = correct.Union(incorrect);

            foreach (var l in testLogs)
            {
                _output.WriteLine($"logs: {l}");
            }

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new IdentifyandAdjustVehicleActivations();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"corrected event: {l}");
            }

            var expected = correct.Select(s => s.EventCode).Where(w => w == (int)DataLoggerEnum.DetectorOn).Count();
            var actual = result.Item2.Count();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Data Check")]
        public async void IdentifyandAdjustVehicleActivationsDataCheckTest()
        {
            var testDetector = _testApproach.Detectors.FirstOrDefault(f => f.DetectorChannel == 2);

            var testLog = new ControllerEventLog()
            {
                SignalIdentifier = _testApproach.Signal.SignalIdentifier,
                Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                EventCode = (int)DataLoggerEnum.DetectorOn,
                EventParam = 2
            };

            _output.WriteLine($"log: {testLog}");

            var testData = Tuple.Create<Approach, IEnumerable<ControllerEventLog>>(_testApproach, new List<ControllerEventLog>() { testLog });

            var sut = new IdentifyandAdjustVehicleActivations();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"corrected event: {l}");
            }

            var expected = new CorrectedDetectorEvent()
            {
                SignalIdentifier = _testApproach.Signal.SignalIdentifier,
                PhaseNumber = _testApproach.ProtectedPhaseNumber,
                Direction = _testApproach.DirectionTypeId,
                DetectorChannel = testDetector.DetectorChannel,
                Timestamp = AtspmMath.AdjustTimeStamp(testLog.Timestamp, _testApproach.Mph ?? 0, testDetector.DistanceFromStopBar ?? 0, testDetector.LatencyCorrection)
            };

            var actual = result.Item2.First();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Null Input")]
        public async void IdentifyandAdjustVehicleActivationsNullInputTest()
        {
            var testData = Tuple.Create<Approach, IEnumerable<ControllerEventLog>>(null, null);

            var sut = new IdentifyandAdjustVehicleActivations();

            var result = await sut.ExecuteAsync(testData);

            Assert.True(result != null);
            Assert.True(result.Item1 == null);
            Assert.True(result.Item2 == null);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "No Data")]
        public async void IdentifyandAdjustVehicleActivationsNoDataTest()
        {
            var testLogs = Enumerable.Range(1, 5).Select(s => new ControllerEventLog()
            {
                SignalIdentifier = "1001",
                Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                EventCode = Random.Shared.Next(1, 50),
                EventParam = 5
            });

            foreach (var l in testLogs)
            {
                _output.WriteLine($"logs: {l}");
            }

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new IdentifyandAdjustVehicleActivations();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var l in result.Item2)
            {
                _output.WriteLine($"corrected event: {l}");
            }

            Assert.True(result != null);
            Assert.True(result.Item1 == _testApproach);
            Assert.True(result.Item2?.Count() == 0);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "From File")]
        public async void IdentifyandAdjustVehicleActivationsFromFileTest()
        {
            var json = File.ReadAllText(new FileInfo(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\IdentifyandAdjustVehicleActivationsTestData.json").FullName);
            var testFile = JsonConvert.DeserializeObject<IdentifyandAdjustVehicleActivationsTestData>(json);

            _output.WriteLine($"Configuration: {testFile.Configuration}");
            _output.WriteLine($"Input: {testFile.Input.Count}");
            _output.WriteLine($"Output: {testFile.Output.Count}");

            var testData = Tuple.Create(testFile.Configuration, testFile.Input.AsEnumerable());

            var sut = new IdentifyandAdjustVehicleActivations();

            var result = await sut.ExecuteAsync(testData);

            var expected = testFile.Output;
            var actual = result.Item2.ToList();

            _output.WriteLine($"expected: {expected.Count}");
            _output.WriteLine($"actual: {actual.Count}");

            Assert.Equivalent(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
