using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.PurdueCoordination;
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
    public class GeneratePurdueCoordinationResultTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public GeneratePurdueCoordinationResultTests(ITestOutputHelper output)
        {
            _output = output;
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

        //#region ICycleArrivals

        //[Fact]
        //[Trait(nameof(CalculateVehicleArrivals), "ArrivalOnGreenPass")]
        //public async void CalculateVehicleArrivalsArrivalOnGreenPassTest()
        //{
        //    var sut = new CalculateVehicleArrivals();

        //    var testEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.2") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.4") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.6") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.8") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.2") }
        //    };

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().TotalArrivalOnGreen;
        //    var expected = 5;

        //    Assert.Equal(expected, actual);
        //}

        //[Fact]
        //[Trait(nameof(CalculateVehicleArrivals), "ArrivalOnYellowPass")]
        //public async void CalculateVehicleArrivalsArrivalOnYellowPassTest()
        //{
        //    var sut = new CalculateVehicleArrivals();

        //    var testEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.2") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.4") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.6") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.8") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:3.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:3.2") }
        //    };

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().TotalArrivalOnYellow;
        //    var expected = 5;

        //    Assert.Equal(expected, actual);
        //}

        //[Fact]
        //[Trait(nameof(CalculateVehicleArrivals), "ArrivalOnRedPass")]
        //public async void CalculateVehicleArrivalsArrivalOnRedPassTest()
        //{
        //    var sut = new CalculateVehicleArrivals();

        //    var testEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.2") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.4") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.6") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.8") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.2") }
        //    };

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().TotalArrivalOnRed;
        //    var expected = 5;

        //    Assert.Equal(expected, actual);
        //}

        //#endregion

        //#region ICycleVolume

        //[Fact]
        //[Trait(nameof(CalculateVehicleArrivals), "TotalDelayPass")]
        //public async void CalculateVehicleArrivalsTotalDelayPassTest()
        //{
        //    var sut = new CalculateVehicleArrivals();

        //    var testEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.2") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.4") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.6") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.8") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.2") }
        //    };

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().TotalDelay;
        //    var expected = 2.5;

        //    Assert.Equal(expected, actual);
        //}

        //[Fact]
        //[Trait(nameof(CalculateVehicleArrivals), "TotalVolumePass")]
        //public async void CalculateVehicleArrivalsTotalVolumePassTest()
        //{
        //    var sut = new CalculateVehicleArrivals();

        //    var testEvents = new List<CorrectedDetectorEvent>
        //    {
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.5") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.5") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:3.0") },
        //        new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:3.5") }
        //    };

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().TotalVolume;
        //    var expected = 6;

        //    Assert.Equal(expected, actual);
        //}

        //#endregion

        //#region ICycle

        //[Fact]
        //[Trait(nameof(CalculateVehicleArrivals), "TotalGreenTimePass")]
        //public async void CalculateVehicleArrivalsTotalGreenTimePassTest()
        //{
        //    var sut = new CalculateVehicleArrivals();

        //    var testEvents = new List<CorrectedDetectorEvent>();

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().TotalGreenTime;
        //    var expected = _redCycles[0].TotalGreenTime;

        //    Assert.Equal(expected, actual);
        //}

        //[Fact]
        //[Trait(nameof(CalculateVehicleArrivals), "TotalYellowTimePass")]
        //public async void CalculateVehicleArrivalsTotalYellowTimePassTest()
        //{
        //    var sut = new CalculateVehicleArrivals();

        //    var testEvents = new List<CorrectedDetectorEvent>();

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().TotalYellowTime;
        //    var expected = _redCycles[0].TotalYellowTime;

        //    Assert.Equal(expected, actual);
        //}

        //[Fact]
        //[Trait(nameof(CalculateVehicleArrivals), "TotalRedTimePass")]
        //public async void CalculateVehicleArrivalsTotalRedTimePassTest()
        //{
        //    var sut = new CalculateVehicleArrivals();

        //    var testEvents = new List<CorrectedDetectorEvent>();

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().TotalRedTime;
        //    var expected = _redCycles[0].TotalRedTime;

        //    Assert.Equal(expected, actual);
        //}

        //[Fact]
        //[Trait(nameof(CalculateVehicleArrivals), "TotalTimePass")]
        //public async void CalculateVehicleArrivalsTotalTimePassTest()
        //{
        //    var sut = new CalculateVehicleArrivals();

        //    var testEvents = new List<CorrectedDetectorEvent>();

        //    var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

        //    var actual = result.First().TotalTime;
        //    var expected = _redCycles[0].TotalTime;

        //    Assert.Equal(expected, actual);
        //}

        //#endregion

        public void Dispose()
        {
        }
    }
}
