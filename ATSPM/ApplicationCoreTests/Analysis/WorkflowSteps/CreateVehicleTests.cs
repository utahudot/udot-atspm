using ApplicationCoreTests.Analysis.TestObjects;
using ApplicationCoreTests.Fixtures;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using AutoFixture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    //internal class DetectorFixture : Fixture
    //{
    //    public DetectorFixture()
    //    {
    //        Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => this.Behaviors.Remove(b));
    //        Behaviors.Add(new OmitOnRecursionBehavior());

    //        this.Customize<Detector>(c => c
    //                .With(w => w.ApproachId, 100)
    //                .With(w => w.DetectorChannel, 50)
    //            );
    //    }
    //}

    public class CreateVehicleTests : IClassFixture<TestApproachFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Approach _testApproach;

        public CreateVehicleTests(ITestOutputHelper output, TestApproachFixture testApproach)
        {
            _output = output;
            _testApproach = testApproach.TestApproach;
        }

        [Fact]
        [Trait(nameof(CreateVehicle), "Signal Filter")]
        public async void CreateVehicleSignalFilterTest()
        {
            var correctDetectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = 2},
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:02:00"), DetectorChannel = 2},

            }.AsEnumerable();

            var incorrectDetectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:03:00"), DetectorChannel = 2},
                new CorrectedDetectorEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:04:00"), DetectorChannel = 2},

            }.AsEnumerable();

            var testEvents = Tuple.Create(_testApproach, correctDetectorEvents.Union(incorrectDetectorEvents));

            var testCyles = Tuple.Create(_testApproach, new List<RedToRedCycle>()
            {
                new RedToRedCycle()
                {
                    LocationIdentifier = _testApproach.Location.LocationIdentifier,
                    PhaseNumber = _testApproach.ProtectedPhaseNumber,
                    Start = DateTime.Parse("4/17/2023 8:00:00"),
                    End = DateTime.Parse("4/17/2023 8:10:00"),
                    GreenEvent = DateTime.Parse("4/17/2023 8:08:00"),
                    YellowEvent = DateTime.Parse("4/17/2023 8:09:00"),
                }
            }.AsEnumerable());

            var testData = Tuple.Create(testEvents, testCyles);

            var sut = new CreateVehicle();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2)
            {
                _output.WriteLine($"vehicle: {v}");
            }

            var expected = correctDetectorEvents.Count();
            var actual = result.Item2.Count();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        //[Fact]
        //[Trait(nameof(CreateVehicle), "Detector Filter")]
        //public async void CreateVehicleDetectorFilterTest()
        //{
        //    var correctDetectorEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Location.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = 2},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Location.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:02:00"), DetectorChannel = 2},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Location.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:00"), DetectorChannel = 2},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Location.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:00"), DetectorChannel = 2},

        //    }.AsEnumerable();

        //    var incorrectDetectorEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Location.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = inccorect.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Location.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:02:00"), DetectorChannel = inccorect.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Location.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:03:00"), DetectorChannel = inccorect.DetectorChannel},
        //        new CorrectedDetectorEvent() { SignalIdentifier = _testApproach.Location.SignalIdentifier, Timestamp = DateTime.Parse("4/17/2023 8:04:00"), DetectorChannel = inccorect.DetectorChannel},

        //    }.AsEnumerable();

        //    var testEvents = new List<Tuple<Detector, IEnumerable<CorrectedDetectorEvent>>>()
        //    {
        //        Tuple.Create(correct, correctDetectorEvents),
        //        Tuple.Create(inccorect, incorrectDetectorEvents)
        //    }.AsEnumerable();

        //    var testCyles = Tuple.Create(_testApproach, new List<RedToRedCycle>()
        //    {
        //        new RedToRedCycle()
        //        {
        //            SignalIdentifier = _testApproach.Location.SignalIdentifier,
        //            PhaseNumber = _testApproach.ProtectedPhaseNumber,
        //            Start = DateTime.Parse("4/17/2023 8:00:00"),
        //            End = DateTime.Parse("4/17/2023 8:10:00"),
        //            GreenEvent = DateTime.Parse("4/17/2023 8:08:00"),
        //            YellowEvent = DateTime.Parse("4/17/2023 8:09:00"),
        //        }
        //    }.AsEnumerable());

        //    var testData = Tuple.Create(testEvents, testCyles);

        //    var sut = new CreateVehicle();

        //    var result = await sut.ExecuteAsync(testData);

        //    _output.WriteLine($"approach: {result.Item1}");

        //    foreach (var v in result.Item2)
        //    {
        //        _output.WriteLine($"vehicle: {v}");
        //    }

        //    var expected = correctDetectorEvents.Count();
        //    var actual = result.Item2.Count();

        //    _output.WriteLine($"expected: {expected}");
        //    _output.WriteLine($"actual: {actual}");

        //    Assert.Equal(expected, actual);
        //}

        [Fact]
        [Trait(nameof(CreateVehicle), "Compare Start")]
        public async void CreateVehicleTestsCompareStartTest()
        {
            var detectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 7:59:00"), DetectorChannel = 2},
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:00:00"), DetectorChannel = 2},
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = 2},

            }.AsEnumerable();

            var testEvents = Tuple.Create(_testApproach, detectorEvents);

            var testCyles = Tuple.Create(_testApproach, new List<RedToRedCycle>()
            {
                new RedToRedCycle()
                {
                    LocationIdentifier = _testApproach.Location.LocationIdentifier,
                    PhaseNumber = _testApproach.ProtectedPhaseNumber,
                    Start = DateTime.Parse("4/17/2023 8:00:00"),
                    End = DateTime.Parse("4/17/2023 8:10:00"),
                    GreenEvent = DateTime.Parse("4/17/2023 8:08:00"),
                    YellowEvent = DateTime.Parse("4/17/2023 8:09:00"),
                }
            }.AsEnumerable());

            var testData = Tuple.Create(testEvents, testCyles);

            var sut = new CreateVehicle();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2)
            {
                _output.WriteLine($"vehicle: {v}");
            }

            var expected = 2;
            var actual = result.Item2.Count();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreateVehicle), "Compare End")]
        public async void CreateVehicleTestsCompareEndTest()
        {
            var detector = _testApproach.Detectors.First();

            var detectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:10:00"), DetectorChannel = detector.DetectorChannel},
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:11:00"), DetectorChannel = detector.DetectorChannel},

            }.AsEnumerable();

            var testEvents = Tuple.Create(_testApproach, detectorEvents);

            var testCyles = Tuple.Create(_testApproach, new List<RedToRedCycle>()
            {
                new RedToRedCycle()
                {
                    LocationIdentifier = _testApproach.Location.LocationIdentifier,
                    PhaseNumber = _testApproach.ProtectedPhaseNumber,
                    Start = DateTime.Parse("4/17/2023 8:00:00"),
                    End = DateTime.Parse("4/17/2023 8:10:00"),
                    GreenEvent = DateTime.Parse("4/17/2023 8:08:00"),
                    YellowEvent = DateTime.Parse("4/17/2023 8:09:00"),
                }
            }.AsEnumerable());

            var testData = Tuple.Create(testEvents, testCyles);

            var sut = new CreateVehicle();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2)
            {
                _output.WriteLine($"vehicle: {v}");
            }

            var expected = 1;
            var actual = result.Item2.Count();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CreateVehicle), "Data Check")]
        public async void CreateVehicleTestsDataCheckTest()
        {
            var detectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = 2},

            }.AsEnumerable();

            var testEvents = Tuple.Create(_testApproach, detectorEvents);

            var testCycle = new RedToRedCycle()
            {
                LocationIdentifier = _testApproach.Location.LocationIdentifier,
                PhaseNumber = _testApproach.ProtectedPhaseNumber,
                Start = DateTime.Parse("4/17/2023 8:00:00"),
                End = DateTime.Parse("4/17/2023 8:10:00"),
                GreenEvent = DateTime.Parse("4/17/2023 8:08:00"),
                YellowEvent = DateTime.Parse("4/17/2023 8:09:00"),
            };

            var testCyles = Tuple.Create(_testApproach, new List<RedToRedCycle>()
            {
                testCycle
            }.AsEnumerable());

            var testData = Tuple.Create(testEvents, testCyles);

            var sut = new CreateVehicle();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2)
            {
                _output.WriteLine($"vehicle: {v}");
            }

            var expected = new Vehicle()
            {
                LocationIdentifier = _testApproach.Location.LocationIdentifier,
                PhaseNumber = _testApproach.ProtectedPhaseNumber,
                Timestamp = DateTime.Parse("4/17/2023 8:01:00"),
                DetectorChannel = 2,
                RedToRedCycle = testCycle
            };
            var actual = result.Item2.First();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equivalent(expected, actual);
        }

        ///// <summary>
        ///// Delay is only calculated for arrival on red
        ///// </summary>
        //[Fact]
        //[Trait(nameof(CreateVehicleTests), "Red Delay")]
        //public async void CreateVehicleTestsRedDelayTest()
        //{
        //    var sut = new CreateVehicleTests();

        //    var testEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
        //    };

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().Delay;
        //    var expected = (_redCycles.First().GreenEvent - testEvents.First().CorrectedTimeStamp).TotalSeconds;

        //    Assert.Equal(expected, actual);
        //}

        ///// <summary>
        ///// Delay is only calculated for arrival on red result should be 0
        ///// </summary>
        //[Fact]
        //[Trait(nameof(CreateVehicleTests), "Green Delay")]
        //public async void CreateVehicleTestsGreenDelayTest()
        //{
        //    var sut = new CreateVehicleTests();

        //    var testEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.5") }
        //    };

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    _output.WriteLine($"{result.First().GreenEvent:yyyy-MM-dd'T'HH:mm:ss.f} - {result.First().CorrectedTimeStamp:yyyy-MM-dd'T'HH:mm:ss.f}");

        //    var actual = result.First().Delay;
        //    var expected = 0;

        //    Assert.Equal(expected, actual);
        //}

        ///// <summary>
        ///// Delay is only calculated for arrival on red result should be 0
        ///// </summary>
        //[Fact]
        //[Trait(nameof(CreateVehicleTests), "Yellow Delay")]
        //public async void CreateVehicleTestsYellowDelayTest()
        //{
        //    var sut = new CreateVehicleTests();

        //    var testEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.5") }
        //    };

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().Delay;
        //    var expected = 0;

        //    Assert.Equal(expected, actual);
        //}

        [Fact]
        [Trait(nameof(CreateVehicle), "Arrival on Red")]
        public async void CreateVehicleTestsArrivalOnRedTest()
        {
            var detectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:01:00"), DetectorChannel = 2},

            }.AsEnumerable();

            var testEvents = Tuple.Create(_testApproach, detectorEvents);

            var testCycle = new RedToRedCycle()
            {
                LocationIdentifier = _testApproach.Location.LocationIdentifier,
                PhaseNumber = _testApproach.ProtectedPhaseNumber,
                Start = DateTime.Parse("4/17/2023 8:00:00"),
                End = DateTime.Parse("4/17/2023 8:10:00"),
                GreenEvent = DateTime.Parse("4/17/2023 8:08:00"),
                YellowEvent = DateTime.Parse("4/17/2023 8:09:00"),
            };

            var testCyles = Tuple.Create(_testApproach, new List<RedToRedCycle>()
            {
                testCycle
            }.AsEnumerable());

            var testData = Tuple.Create(testEvents, testCyles);

            var sut = new CreateVehicle();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2)
            {
                _output.WriteLine($"vehicle: {v}");
            }

            var condition = result.Item2.First().ArrivalType == ArrivalType.ArrivalOnRed;

            _output.WriteLine($"condition: {condition}");

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CreateVehicle), "Arrival on Green")]
        public async void CreateVehicleTestsArrivalOnGreenTest()
        {
            var detectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:08:30"), DetectorChannel = 2},

            }.AsEnumerable();

            var testEvents = Tuple.Create(_testApproach, detectorEvents);

            var testCycle = new RedToRedCycle()
            {
                LocationIdentifier = _testApproach.Location.LocationIdentifier,
                PhaseNumber = _testApproach.ProtectedPhaseNumber,
                Start = DateTime.Parse("4/17/2023 8:00:00"),
                End = DateTime.Parse("4/17/2023 8:10:00"),
                GreenEvent = DateTime.Parse("4/17/2023 8:08:00"),
                YellowEvent = DateTime.Parse("4/17/2023 8:09:00"),
            };

            var testCyles = Tuple.Create(_testApproach, new List<RedToRedCycle>()
            {
                testCycle
            }.AsEnumerable());

            var testData = Tuple.Create(testEvents, testCyles);

            var sut = new CreateVehicle();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2)
            {
                _output.WriteLine($"vehicle: {v}");
            }

            var condition = result.Item2.First().ArrivalType == ArrivalType.ArrivalOnGreen;

            _output.WriteLine($"condition: {condition}");

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CreateVehicle), "Arrival on Yellow")]
        public async void CreateVehicleTestsArrivalOnYellowTest()
        {
            var detectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { LocationIdentifier = _testApproach.Location.LocationIdentifier, PhaseNumber = 2, Timestamp = DateTime.Parse("4/17/2023 8:09:30"), DetectorChannel = 2},

            }.AsEnumerable();

            var testEvents = Tuple.Create(_testApproach, detectorEvents);

            var testCycle = new RedToRedCycle()
            {
                LocationIdentifier = _testApproach.Location.LocationIdentifier,
                PhaseNumber = _testApproach.ProtectedPhaseNumber,
                Start = DateTime.Parse("4/17/2023 8:00:00"),
                End = DateTime.Parse("4/17/2023 8:10:00"),
                GreenEvent = DateTime.Parse("4/17/2023 8:08:00"),
                YellowEvent = DateTime.Parse("4/17/2023 8:09:00"),
            };

            var testCyles = Tuple.Create(_testApproach, new List<RedToRedCycle>()
            {
                testCycle
            }.AsEnumerable());

            var testData = Tuple.Create(testEvents, testCyles);

            var sut = new CreateVehicle();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2)
            {
                _output.WriteLine($"vehicle: {v}");
            }

            var condition = result.Item2.First().ArrivalType == ArrivalType.ArrivalOnYellow;

            _output.WriteLine($"condition: {condition}");

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CreateVehicle), "Null Input")]
        public async void CreateVehicleNullInputTest()
        {
            var testEvents = Tuple.Create<Approach, IEnumerable<CorrectedDetectorEvent>>(null, null);

            var testCyles = Tuple.Create<Approach, IEnumerable<RedToRedCycle>>(null, null);

            var testData = Tuple.Create(testEvents, testCyles);

            var sut = new CreateVehicle();

            var result = await sut.ExecuteAsync(testData);

            Assert.True(result != null);
            Assert.True(result.Item1 == null);
            Assert.True(result.Item2 == null);
        }

        [Fact]
        [Trait(nameof(CreateVehicle), "No Data")]
        public async void CreateVehicleNoDataTest()
        {
            var detectorEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent() { LocationIdentifier = "1001", Timestamp = DateTime.Parse("4/17/2023 8:09:30"), DetectorChannel = 2},

            }.AsEnumerable();

            var testEvents = Tuple.Create(_testApproach, detectorEvents);

            var testCycle = new RedToRedCycle()
            {
                LocationIdentifier = _testApproach.Location.LocationIdentifier,
                PhaseNumber = _testApproach.ProtectedPhaseNumber,
                Start = DateTime.Parse("4/17/2023 8:00:00"),
                End = DateTime.Parse("4/17/2023 8:10:00"),
                GreenEvent = DateTime.Parse("4/17/2023 8:08:00"),
                YellowEvent = DateTime.Parse("4/17/2023 8:09:00"),
            };

            var testCyles = Tuple.Create(_testApproach, new List<RedToRedCycle>()
            {
                testCycle
            }.AsEnumerable());

            var testData = Tuple.Create(testEvents, testCyles);

            var sut = new CreateVehicle();

            var result = await sut.ExecuteAsync(testData);

            _output.WriteLine($"approach: {result.Item1}");

            foreach (var v in result.Item2)
            {
                _output.WriteLine($"vehicle: {v}");
            }

            Assert.True(result != null);
            Assert.True(result.Item1 == _testApproach);
            Assert.True(result.Item2?.Count() == 0);
        }

        [Fact]
        [Trait(nameof(CreateVehicle), "From File")]
        public async void CreateVehicleFromFileTest()
        {
            Assert.True(false);
        }

        public void Dispose()
        {
        }
    }
}
