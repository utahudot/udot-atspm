using ATSPM.Application;
using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class IdentifyandAdjustVehicleActivationsTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public IdentifyandAdjustVehicleActivationsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Signal Filter")]
        public async void IdentifyandAdjustVehicleActivationsSignalFilterTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            var testData = Enumerable.Range(1, 10).Select(s => GenerateDetectorEvents(Random.Shared.Next(1, 5).ToString(), 1, 10)).ToList();

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Select(s => s.Detector.Approach.Signal.SignalIdentifier).Distinct().OrderBy(o => o);
            var expected = testData.SelectMany(s => s.Item2).Select(s => s.SignalIdentifier).Distinct().OrderBy(o => o);

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Detector Filter")]
        public async void IdentifyandAdjustVehicleActivationsDetectorFilterTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            var detChannel = 1;

            var testData = Enumerable.Range(1, 1).Select(s => GenerateDetectorEvents(Random.Shared.Next(1, 5).ToString(), detChannel, 10))
                .Select(s => Tuple.Create(s.Item1, s.Item2.Select(l => new ControllerEventLog
                {
                    SignalIdentifier = l.SignalIdentifier,
                    EventCode = l.EventCode,
                    EventParam = Random.Shared.Next(1, 5),
                    TimeStamp = l.TimeStamp
                }).ToList().AsEnumerable())).ToList();

            var result = await sut.ExecuteAsync(testData);

            var expected = testData.First().Item2.Where(w => w.EventParam == detChannel).Count();
            var actual = result.Where(w => w.Detector.DetChannel == detChannel).Count();

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Code Filter")]
        public async void IdentifyandAdjustVehicleActivationsCodeFilterTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            var testData = Enumerable.Range(1, 1).Select(s => GenerateDetectorEvents(Random.Shared.Next(1, 5).ToString(), 1, 10))
                .Select(s => Tuple.Create(s.Item1, s.Item2.Select(l => new ControllerEventLog
                {
                    SignalIdentifier = l.SignalIdentifier,
                    EventCode = Random.Shared.Next(82, 85),
                    EventParam = l.EventParam,
                    TimeStamp = l.TimeStamp
                }).ToList().AsEnumerable())).ToList();

            var result = await sut.ExecuteAsync(testData);

            var expected = testData.First().Item2.Where(w => w.EventCode == 82).Count();
            var actual = result.Count();

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Data Check")]
        public async void IdentifyandAdjustVehicleActivationsDataCheckTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            string signal = "1001";
            int channel = 1;

            var testData = Enumerable.Range(1, 1).Select(s => GenerateDetectorEvents(signal, channel, 10)).ToList();

            var result = await sut.ExecuteAsync(testData);

            var actual = result.First().CorrectedTimeStamp;
            var expected = AtspmMath.AdjustTimeStamp(testData.First().Item2.First().TimeStamp,
                testData.First().Item1.Approach?.Mph ?? 0,
                testData.First().Item1.DistanceFromStopBar ?? 0,
                testData.First().Item1.LatencyCorrection);

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Valid Latency Correnction")]
        public async void IdentifyandAdjustVehicleActivationsValidLatencyCorrectionTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            var testData = Enumerable.Range(1, 1).Select(s => GenerateDetectorEvents("1001", 1, 10)).ToList();

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Select(s => s.CorrectedTimeStamp).OrderBy(o => o);
            var expected = testData.SelectMany(s =>
            s.Item2.Select(t => AtspmMath.AdjustTimeStamp(t.TimeStamp, s.Item1?.Approach?.Mph ?? 0, s.Item1.DistanceFromStopBar ?? 0, s.Item1.LatencyCorrection)))
                .OrderBy(o => o);

            _output.WriteLine($"count1: {actual.Count()}");
            _output.WriteLine($"count2: {expected.Count()}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(IdentifyandAdjustVehicleActivations), "Invalid Latency Correnction")]
        public async void IdentifyandAdjustVehicleActivationsInvalidLatencyCorrectionTest()
        {
            var sut = new IdentifyandAdjustVehicleActivations();

            var testData = Enumerable.Range(1, 1).Select(s => GenerateDetectorEvents("1001", 1, 10)).ToList();

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Select(s => s.CorrectedTimeStamp).OrderBy(o => o);
            var expected = testData.SelectMany(s => s.Item2.Select(t => t.TimeStamp)).OrderBy(o => o);

            _output.WriteLine($"count1: {actual.Count()}");
            _output.WriteLine($"count2: {expected.Count()}");

            Assert.NotEqual(expected, actual);
        }

        private Tuple<Detector, IEnumerable<ControllerEventLog>> GenerateDetectorEvents(string signalId, int detChannel, int logCount)
        {
            var d = new Detector()
            {
                DetChannel = detChannel,
                DistanceFromStopBar = 340,
                LatencyCorrection = 1.2,
                Approach = new Approach()
                {
                    ProtectedPhaseNumber = 2,
                    DirectionTypeId = DirectionTypes.NB,
                    Mph = 45,
                    Signal = new Signal()
                    {
                        SignalIdentifier = signalId
                    }
                }
            };

            var logs = Enumerable.Range(1, logCount).Select(s => new ControllerEventLog()
            {
                SignalIdentifier = signalId,
                TimeStamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                EventCode = 82,
                EventParam = detChannel
            }).ToList();

            return Tuple.Create<Detector, IEnumerable<ControllerEventLog>>(d, logs);
        }

        public void Dispose()
        {
        }
    }
}
