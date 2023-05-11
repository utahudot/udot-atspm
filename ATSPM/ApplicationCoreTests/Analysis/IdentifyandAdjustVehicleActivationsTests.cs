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
    public class IdentifyandAdjustVehicleActivationsTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Detector _detector;

        private StringWriter _consoleOut = new StringWriter();

        public IdentifyandAdjustVehicleActivationsTests(ITestOutputHelper output)
        {
            _output = output;

            _detector = new Detector()
            {
                DetChannel = 2,
                DistanceFromStopBar = 340,
                LatencyCorrection = 1.2,
                Approach = new Approach() 
                { 
                    Mph = 45,
                    Signal = new Signal()
                    {
                        SignalId = "1001"
                    }
                }
            };
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Signal Filter")]
        public async void IdentifyandAdjustVehicleActivationsSignalFilterTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:0.2"), EventCode = 82, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:2.3"), EventCode = 82, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:00:6.3"), EventCode = 82, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1002", Timestamp = DateTime.Parse("4/17/2023 8:00:7.1"), EventCode = 82, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 8:00:7.3"), EventCode = 82, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1003", Timestamp = DateTime.Parse("4/17/2023 8:00:10.5"), EventCode = 82, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1004", Timestamp = DateTime.Parse("4/17/2023 8:00:11.9"), EventCode = 82, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1004", Timestamp = DateTime.Parse("4/17/2023 8:00:12.2"), EventCode = 82, EventParam = _detector.DetChannel}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<Detector, IEnumerable<ControllerEventLog>>(_detector, testData));

            var condition = result.All(a => a.SignalId == _detector.Approach.Signal.SignalId);

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Detector Filter")]
        public async void IdentifyandAdjustVehicleActivationsDetectorFilterTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:0.2"), EventCode = 82, EventParam = 1},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:2.3"), EventCode = 82, EventParam = 1},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:6.3"), EventCode = 82, EventParam = 1},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:7.1"), EventCode = 82, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:7.3"), EventCode = 82, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:10.5"), EventCode = 82, EventParam = 2},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:11.9"), EventCode = 82, EventParam = 3},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:12.2"), EventCode = 82, EventParam = 3},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:12.2"), EventCode = 82, EventParam = 3}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<Detector, IEnumerable<ControllerEventLog>>(_detector, testData));

            var condition = result.All(a => a.DetChannel == _detector.DetChannel);

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Code Filter")]
        public async void IdentifyandAdjustVehicleActivationsCodeFilterTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:0.2"), EventCode = 1, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:2.3"), EventCode = 2, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:6.3"), EventCode = 3, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:7.1"), EventCode = 4, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:7.3"), EventCode = 82, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:10.5"), EventCode = 82, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:11.9"), EventCode = 82, EventParam = _detector.DetChannel},
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:12.2"), EventCode = 82, EventParam = _detector.DetChannel}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<Detector, IEnumerable<ControllerEventLog>>(_detector, testData));

            var condition = result.Count() == 4;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Latency Correnction")]
        public async void IdentifyandAdjustVehicleActivationsLatencyCorrectionTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:0.2"), EventCode = 82, EventParam = _detector.DetChannel}
            };

            //

            var result = await sut.ExecuteAsync(Tuple.Create<Detector, IEnumerable<ControllerEventLog>>(_detector, testData));

            var actual = result.First().TimeStamp;
            var expected = DateTime.Parse("4/17/2023 8:00:4.15");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Data Check")]
        public async void IdentifyandAdjustVehicleActivationsDataCheckTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            var testData = new List<ControllerEventLog>
            {
                new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:00:0.2"), EventCode = 82, EventParam = _detector.DetChannel}
            };

            var result = await sut.ExecuteAsync(Tuple.Create<Detector, IEnumerable<ControllerEventLog>>(_detector, testData));

            var actual = result.First();
            var expected = _detector;

            Assert.Equal(expected.DetChannel, actual.DetChannel);
            Assert.Equal(expected.Approach.Signal.SignalId, actual.SignalId);
        }

        public void Dispose()
        {
        }
    }
}
