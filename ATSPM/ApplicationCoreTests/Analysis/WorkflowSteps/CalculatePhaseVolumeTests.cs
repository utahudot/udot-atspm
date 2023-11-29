using ApplicationCoreTests.Analysis.TestObjects;
using ApplicationCoreTests.Fixtures;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using AutoFixture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using ATSPM.Domain.Extensions;
using ATSPM.Application;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class CalculatePhaseVolumeTests : IClassFixture<TestApproachFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Approach _testApproach;

        public CalculatePhaseVolumeTests(ITestOutputHelper output, TestApproachFixture testApproach)
        {
            _output = output;
            _testApproach = testApproach.TestApproach;
        }

        [Fact]
        [Trait(nameof(CalculatePhaseVolume), "Signal Filter")]
        public async void CalculatePhaseVolumeSignalFilterTest()
        {
            var correctDetectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = 2},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:02:00"), DetectorChannel = 2},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:00"), DetectorChannel = 2},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:00"), DetectorChannel = 2},

            }.AsEnumerable();

            var incorrectDetectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:05:00"), DetectorChannel = 2},
                new CorrectedDetectorEvent() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:06:00"), DetectorChannel = 2},
                new CorrectedDetectorEvent() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:07:00"), DetectorChannel = 2},
                new CorrectedDetectorEvent() { SignalIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:08:00"), DetectorChannel = 2},

            }.AsEnumerable();

            var testEvents = correctDetectorEvents.Union(incorrectDetectorEvents);

            var testData = Tuple.Create(_testApproach, testEvents);

            var sut = new CalculatePhaseVolume();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2.Segments)
            {
                _output.WriteLine($"volume: {v}");
            }

            var expected = correctDetectorEvents.Count();
            var actual = result.Item2.Segments.SelectMany(m => m.DetectorEvents).Count();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CalculatePhaseVolume), "Detector Filter")]
        public async void CalculatePhaseVolumeDetectorFilterTest()
        {
            var correct = 2;
            var inccorect = 4;

            var correctDetectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = correct},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:02:00"), DetectorChannel = correct},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:00"), DetectorChannel = correct},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:00"), DetectorChannel = correct},

            }.AsEnumerable();

            var incorrectDetectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:05:00"), DetectorChannel = inccorect},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:06:00"), DetectorChannel = inccorect},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:07:00"), DetectorChannel = inccorect},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:08:00"), DetectorChannel = inccorect},

            }.AsEnumerable();

            var testEvents = correctDetectorEvents.Union(incorrectDetectorEvents);

            var testData = Tuple.Create(_testApproach, testEvents);

            var sut = new CalculatePhaseVolume();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2.Segments)
            {
                _output.WriteLine($"volume: {v}");
            }

            var expected = correctDetectorEvents.Count();
            var actual = result.Item2.Segments.SelectMany(m => m.DetectorEvents).Count();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CalculatePhaseVolume), "Timeline Check")]
        public async void CalculatePhaseVolumeTimelineCheckTest()
        {
            var detector = _testApproach.Detectors.First();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:02:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:00"), DetectorChannel = detector.DetectorChannel},

            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testEvents);

            var sut = new CalculatePhaseVolume();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2.Segments)
            {
                _output.WriteLine($"volume: {v}");
            }

            //var expected = new Volumes()
            //{
            //    SignalIdentifier = _testApproach.Signal.SignalIdentifier,
            //    PhaseNumber = _testApproach.ProtectedPhaseNumber,
            //    Direction = _testApproach.DirectionTypeId,
            //    Start = DateTime.Parse("4/17/2023 8:00:00"),
            //    End = DateTime.Parse("4/17/2023 8:15:00")
            //};

            var expected = new Volumes(DateTime.Parse("4/17/2023 8:00:00"), DateTime.Parse("4/17/2023 8:15:00"), TimeSpan.FromMinutes(15))
            {
                SignalIdentifier = _testApproach.Signal.SignalIdentifier,
                PhaseNumber = _testApproach.ProtectedPhaseNumber,
                Direction = _testApproach.DirectionTypeId
            };

            var actual = result.Item2;

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected.SignalIdentifier, actual.SignalIdentifier);
            Assert.Equal(expected.PhaseNumber, actual.PhaseNumber);
            Assert.Equal(expected.Direction, actual.Direction);
            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.End, actual.End);
        }

        [Fact]
        [Trait(nameof(CalculatePhaseVolume), "Data Check")]
        public async void CalculatePhaseVolumeDataCheckTest()
        {
            var detector = _testApproach.Detectors.First();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:02:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:00"), DetectorChannel = detector.DetectorChannel},

            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testEvents);

            var sut = new CalculatePhaseVolume();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2.Segments)
            {
                _output.WriteLine($"volume: {v}");
            }

            var expected = new Volume()
            {
                SignalIdentifier = _testApproach.Signal.SignalIdentifier,
                PhaseNumber = _testApproach.ProtectedPhaseNumber,
                Direction = _testApproach.DirectionTypeId,
                Start = DateTime.Parse("4/17/2023 8:00:00"),
                End = DateTime.Parse("4/17/2023 8:15:00")
            };

            expected.DetectorEvents.AddRange(testEvents);

            var actual = result.Item2.Segments.First();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected.SignalIdentifier, actual.SignalIdentifier);
            Assert.Equal(expected.PhaseNumber, actual.PhaseNumber);
            Assert.Equal(expected.Direction, actual.Direction);
            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.End, actual.End);
            Assert.Equal(expected.DetectorCount, actual.DetectorCount);
        }

        [Fact]
        [Trait(nameof(CalculatePhaseVolume), "Start/End Check")]
        public async void CalculatePhaseStartEndCheckTest()
        {
            var detector = _testApproach.Detectors.First();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:14:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:15:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:16:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:20:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:45:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:50:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:55:00"), DetectorChannel = detector.DetectorChannel},

            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testEvents);

            var sut = new CalculatePhaseVolume();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            _output.WriteLine($"volumes: {result.Item2}");

            foreach (var v in result.Item2.Segments)
            {
                _output.WriteLine($"start: {v.Start}");
                _output.WriteLine($"end: {v.End}");
            }

            Assert.Equal(DateTime.Parse("4/17/2023 8:00:00"), result.Item2.Start);
            Assert.Equal(DateTime.Parse("4/17/2023 9:00:00"), result.Item2.End);
        }

        [Fact]
        [Trait(nameof(CalculatePhaseVolume), "Time Segment Check")]
        public async void CalculatePhaseTimeSegmentCheckTest()
        {
            var detector = _testApproach.Detectors.First();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:14:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:15:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:16:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:20:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:45:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:50:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Signal.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:55:00"), DetectorChannel = detector.DetectorChannel},

            }.AsEnumerable();

            var testData = Tuple.Create(_testApproach, testEvents);

            var sut = new CalculatePhaseVolume();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2.Segments)
            {
                _output.WriteLine($"volume: {v}");
            }

            Assert.Collection(result.Item2.Segments, 
                a => Assert.True(a.DetectorCount == 2),
                a => Assert.True(a.DetectorCount == 3),
                a => Assert.True(a.DetectorCount == 0),
                a => Assert.True(a.DetectorCount == 3));
        }

        [Fact]
        [Trait(nameof(CalculatePhaseVolume), "Null Input")]
        public async void CalculatePhaseVolumeNullInputTest()
        {
            var testData = Tuple.Create<Approach, IEnumerable<CorrectedDetectorEvent>>(null, null);

            var sut = new CalculatePhaseVolume();

            var result = await sut.ExecuteAsync(testData);

            Assert.True(result != null);
            Assert.True(result.Item1 == null);
            Assert.True(result.Item2 == null);
        }

        [Fact]
        [Trait(nameof(CalculatePhaseVolume), "No Data")]
        public async void CalculatePhaseVolumeNoDataTest()
        {
            var testLogs = Enumerable.Range(1, 5).Select(s => new CorrectedDetectorEvent()
            {
                SignalIdentifier = "1001",
                Timestamp = DateTime.Now.AddMilliseconds(Random.Shared.Next(1, 1000)),
                DetectorChannel = 5
            });

            var testData = Tuple.Create(_testApproach, testLogs);

            var sut = new CalculatePhaseVolume();

            var result = await sut.ExecuteAsync(testData);

            Assert.True(result != null);
            Assert.True(result.Item1 == _testApproach);
            Assert.True(result.Item2 == null);
        }

        [Theory]
        [InlineData(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculatePhaseVolumeTestData1.json")]
        [InlineData(@"C:\Users\christianbaker\source\repos\udot-atspm\ATSPM\ApplicationCoreTests\Analysis\TestData\CalculatePhaseVolumeTestData2.json")]
        [Trait(nameof(CalculatePhaseVolume), "From File")]
        public async void CalculatePhaseVolumeFromFileTest(string file)
        {
            var json = File.ReadAllText(new FileInfo(file).FullName);
            var testFile = JsonConvert.DeserializeObject<CalculatePhaseVolumeTestData>(json);

            _output.WriteLine($"Configuration: {testFile.Configuration}");
            _output.WriteLine($"Input: {testFile.Input.Count}");
            _output.WriteLine($"Output: {testFile.Output.Segments.Count}");

            var testData = Tuple.Create(testFile.Configuration, testFile.Input.AsEnumerable());

            var sut = new CalculatePhaseVolume();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"result: {result.Item2}");

            var test = result.Item2.Segments.GetPeakVolumes(60 / Convert.ToInt32(result.Item2.SegmentSpan.TotalMinutes)).Sum(s => s.DetectorCount);
            _output.WriteLine($"test: {test}");

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
