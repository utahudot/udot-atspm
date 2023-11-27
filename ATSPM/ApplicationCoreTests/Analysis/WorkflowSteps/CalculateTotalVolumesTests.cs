using ApplicationCoreTests.Analysis.TestObjects;
using ApplicationCoreTests.Fixtures;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using AutoFixture;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class CalculateTotalVolumesTests : IClassFixture<TestSignalFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Signal _testSignal;

        public CalculateTotalVolumesTests(ITestOutputHelper output, TestSignalFixture testSignal)
        {
            _output = output;
            _testSignal = testSignal.TestSignal;
        }

        private Volumes GenerateVolumes(string signalIdentifier, int phaseNumber, int detectorChannel, DirectionTypes direction, DateTime start, DateTime end, int count)
        {
            var correctedEventFixture = new Fixture();
            correctedEventFixture.Customize<CorrectedDetectorEvent>(c =>
            {
                return c.With(w => w.SignalIdentifier, signalIdentifier)
                .With(w => w.PhaseNumber, phaseNumber)
                .With(w => w.DetectorChannel, detectorChannel)
                .With(w => w.Direction, direction);
            });
            correctedEventFixture.Customizations.Add(new RandomDateTimeSequenceGenerator(start, end));

            var events = correctedEventFixture.CreateMany<CorrectedDetectorEvent>(count);

            var result = new Volumes(events, TimeSpan.FromMinutes(15))
            {
                SignalIdentifier = signalIdentifier,
                PhaseNumber = phaseNumber,
                Direction = direction,
            };

            result.Segments.ToList().ForEach(f =>
            {
                f.SignalIdentifier = signalIdentifier;
                f.PhaseNumber = phaseNumber;
                f.Direction = direction;
                f.DetectorEvents.AddRange(events.Where(w => f.InRange(w)));
            });

            return result;
        }

        [Fact]
        [Trait(nameof(CalculateTotalVolumes), "Signal Filter")]
        public async void CalculateTotalVolumesSignalFilterTest()
        {
            //var correctDetectorEvents = new List<CorrectedDetectorEvent>
            //{
            //    new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = 2},
            //    new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:02:00"), DetectorChannel = 2},
            //    new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:00"), DetectorChannel = 2},
            //    new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:00"), DetectorChannel = 2},

            //}.AsEnumerable();

            //var incorrectDetectorEvents = new List<CorrectedDetectorEvent>
            //{
            //    new CorrectedDetectorEvent() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:05:00"), DetectorChannel = 2},
            //    new CorrectedDetectorEvent() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:06:00"), DetectorChannel = 2},
            //    new CorrectedDetectorEvent() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:07:00"), DetectorChannel = 2},
            //    new CorrectedDetectorEvent() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:08:00"), DetectorChannel = 2},

            //}.AsEnumerable();

            //var testEvents = correctDetectorEvents.Union(incorrectDetectorEvents);

            //var testData = Tuple.Create(_testApproach, testEvents);

            //var sut = new CalculateTotalVolumes();

            //var result = await sut.ExecuteAsync(testData);

            //_output.WriteLine($"approach: {result.Item1}");

            //foreach (var v in result.Item2)
            //{
            //    _output.WriteLine($"volume: {v}");
            //}

            //var expected = correctDetectorEvents.Count();
            //var actual = result.Item2.SelectMany(m => m).Count();

            //_output.WriteLine($"expected: {expected}");
            //_output.WriteLine($"actual: {actual}");

            //Assert.Equal(expected, actual);
        }

        //[Fact]
        //[Trait(nameof(CalculateTotalVolumes), "Detector Filter")]
        //public async void CalculateTotalVolumesDetectorFilterTest()
        //{
        //    var correct = _testApproach.Detectors.First();
        //    var inccorect = new DetectorFixture().Create<Detector>();

        //    var correctDetectorEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = correct.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:02:00"), DetectorChannel = correct.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:00"), DetectorChannel = correct.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:00"), DetectorChannel = correct.DetectorChannel},

        //    }.AsEnumerable();

        //    var incorrectDetectorEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:05:00"), DetectorChannel = inccorect.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:06:00"), DetectorChannel = inccorect.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:07:00"), DetectorChannel = inccorect.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:08:00"), DetectorChannel = inccorect.DetectorChannel},

        //    }.AsEnumerable();

        //    var testEvents = correctDetectorEvents.Union(incorrectDetectorEvents);

        //    var testData = Tuple.Create(_testApproach, testEvents);

        //    var sut = new CalculateTotalVolumes();

        //    var result = await sut.ExecuteAsync(testData);

        //    _output.WriteLine($"approach: {result.Item1}");

        //    foreach (var v in result.Item2)
        //    {
        //        _output.WriteLine($"volume: {v}");
        //    }

        //    var expected = correctDetectorEvents.Count();
        //    var actual = result.Item2.SelectMany(m => m).Count();

        //    _output.WriteLine($"expected: {expected}");
        //    _output.WriteLine($"actual: {actual}");

        //    Assert.Equal(expected, actual);
        //}

        [Fact]
        [Trait(nameof(CalculateTotalVolumes), "Data Check")]
        public async void CalculateTotalVolumesDataCheckTest()
        {
            var start = DateTime.Parse("4/17/2023 8:00:00");
            var end = DateTime.Parse("4/17/2023 9:00:00");

            var approachP = _testSignal.Approaches.FirstOrDefault(f => f.Id == 2880);
            var approachO = _testSignal.Approaches.FirstOrDefault(f => f.Id == 2882);

            var primaryVolumes = GenerateVolumes(_testSignal.SignalIdentifier, approachP.ProtectedPhaseNumber, 2, approachP.DirectionTypeId, start, end, 10);
            var opposingVolumes = GenerateVolumes(_testSignal.SignalIdentifier, approachO.ProtectedPhaseNumber, 6, approachO.DirectionTypeId, start, end, 10);

            var t1 = Tuple.Create(approachP, primaryVolumes);
            var t2 = Tuple.Create(approachO, opposingVolumes);

            var testData = Tuple.Create(t1, t2);

            var sut = new CalculateTotalVolumes();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");
            _output.WriteLine($"total volume: {result.Item2}");

            foreach (var s in result.Item2.Segments)
            {
                _output.WriteLine($"segments: {s}");
            }

            //var expected = new TotalVolumes()
            //{
            //    SignalIdentifier = approachP.Signal.SignalIdentifier,
            //    Start = start,
            //    End = end,

            //};

            //expected.AddRange(testEvents);

            //var actual = result.Item2.First();

            //_output.WriteLine($"expected: {expected}");
            //_output.WriteLine($"actual: {actual}");

            //Assert.Equivalent(expected, actual);

            Assert.True(false);
        }

        //[Fact]
        //[Trait(nameof(CalculateTotalVolumes), "Start/End Check")]
        //public async void CalculatePhaseStartEndCheckTest()
        //{
        //    var detector = _testApproach.Detectors.First();

        //    var testEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:14:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:15:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:16:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:20:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:45:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:50:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:55:00"), DetectorChannel = detector.DetectorChannel},

        //    }.AsEnumerable();

        //    var testData = Tuple.Create(_testApproach, testEvents);

        //    var sut = new CalculateTotalVolumes();

        //    var result = await sut.ExecuteAsync(testData);

        //    _output.WriteLine($"approach: {result.Item1}");

        //    foreach (var v in result.Item2)
        //    {
        //        _output.WriteLine($"volume: {v}");
        //    }

        //    Assert.Equal(DateTime.Parse("4/17/2023 8:00:00"), result.Item2.Start);
        //    Assert.Equal(DateTime.Parse("4/17/2023 9:00:00"), result.Item2.End);
        //}

        //[Fact]
        //[Trait(nameof(CalculateTotalVolumes), "Time Segment Check")]
        //public async void CalculatePhaseTimeSegmentCheckTest()
        //{
        //    var detector = _testApproach.Detectors.First();

        //    var testEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:14:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:15:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:16:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:20:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:45:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:50:00"), DetectorChannel = detector.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:55:00"), DetectorChannel = detector.DetectorChannel},

        //    }.AsEnumerable();

        //    var testData = Tuple.Create(_testApproach, testEvents);

        //    var sut = new CalculateTotalVolumes();

        //    var result = await sut.ExecuteAsync(testData);

        //    _output.WriteLine($"approach: {result.Item1}");

        //    foreach (var v in result.Item2)
        //    {
        //        _output.WriteLine($"volume: {v}");
        //    }

        //    Assert.Collection(result.Item2, 
        //        a => Assert.True(a.Count == 2),
        //        a => Assert.True(a.Count == 3),
        //        a => Assert.True(a.Count == 0),
        //        a => Assert.True(a.Count == 3));
        //}

        //[Fact]
        //[Trait(nameof(CalculateTotalVolumes), "Null Input")]
        //public async void CalculateTotalVolumesNullInputTest()
        //{
        //    var testData = Tuple.Create<Approach, IEnumerable<CorrectedDetectorEvent>>(null, null);

        //    var sut = new CalculateTotalVolumes();

        //    var result = await sut.ExecuteAsync(testData);

        //    Assert.True(result != null);
        //    Assert.True(result.Item1 == null);
        //    Assert.True(result.Item2 == null);
        //}

        //[Fact]
        //[Trait(nameof(CalculateTotalVolumes), "No Data")]
        //public async void CalculateTotalVolumesNoDataTest()
        //{
        //    var testLogs = Enumerable.Range(1, 5).Select(s => new CorrectedDetectorEvent()
        //    {
        //        SignalIdentifier = "1001",
        //        Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
        //        DetectorChannel = 5
        //    });

        //    var testData = Tuple.Create(_testApproach, testLogs);

        //    var sut = new CalculateTotalVolumes();

        //    var result = await sut.ExecuteAsync(testData);

        //    Assert.True(result != null);
        //    Assert.True(result.Item1 == _testApproach);
        //    Assert.True(result.Item2 == null);
        //}

        [Theory]
        [InlineData(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculateTotalVolumesTestData1.json")]
        [Trait(nameof(CalculateTotalVolumes), "From File")]
        public async void CalculateTotalVolumesFromFileTest(string file)
        {
            var json = File.ReadAllText(new FileInfo(file).FullName);
            var testFile = JsonConvert.DeserializeObject<CalculateTotalVolumeTestData>(json);

            _output.WriteLine($"Configuration: {testFile.Configuration}");
            _output.WriteLine($"Input: {testFile.Input.Count}");
            _output.WriteLine($"Output: {testFile.Output.Segments.Count}");

            var t1 = Tuple.Create(testFile.Configuration[0], testFile.Input[0]);
            var t2 = Tuple.Create(testFile.Configuration[1], testFile.Input[1]);

            var testData = Tuple.Create(t1, t2);

            var sut = new CalculateTotalVolumes();

            var result = await sut.ExecuteAsync(testData);

            var expected = testFile.Output;
            var actual = result.Item2;

            //_output.WriteLine($"expected: {expected.Count}");
            //_output.WriteLine($"actual: {actual.Count}");

            Assert.Equivalent(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
