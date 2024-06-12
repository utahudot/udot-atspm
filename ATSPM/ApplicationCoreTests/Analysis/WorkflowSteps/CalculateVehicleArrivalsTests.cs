#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - %Namespace%/CalculateVehicleArrivalsTests.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.PurdueCoordination;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class CalculateVehicleArrivalsTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly List<RedToRedCycle> _redCycles;
        private readonly Detector _detector;

        public CalculateVehicleArrivalsTests(ITestOutputHelper output)
        {
            _output = output;

            _redCycles = new List<RedToRedCycle>
            {
                new RedToRedCycle()
                {
                    locationIdentifier = "1001",
                    PhaseNumber = 1,
                    Start = DateTime.Parse("4/17/2023 8:00:0.1"),
                    GreenEvent = DateTime.Parse("4/17/2023 8:00:1.1"),
                    YellowEvent = DateTime.Parse("4/17/2023 8:00:2.1"),
                    End = DateTime.Parse("4/17/2023 8:00:3.1")
                }
            };

            _detector = new Detector()
            {
                DetectorChannel = 1,
                DistanceFromStopBar = 340,
                LatencyCorrection = 1.2,
                Approach = new Approach()
                {
                    ProtectedPhaseNumber = 2,
                    DirectionTypeId = DirectionTypes.NB,
                    Mph = 45,
                    Location = new Location()
                    {
                        locationIdentifier = "1001"
                    }
                }
            };
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "Compare Location Pass")]
        public async void CalculateVehicleArrivalsCompareLocationPassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result[0].Vehicles.Count() == 1;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "Compare Location Fail")]
        public async void CalculateVehicleArrivalsCompareLocationFailTest()
        {
            var sut = new CalculateVehicleArrivals();

            _redCycles[0].locationIdentifier = "1002";

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result[0].Vehicles.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "Compare Start Pass")]
        public async void CalculateVehicleArrivalsCompareStartPassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result.Count() == 1;

            Assert.True(condition);

            Assert.Equal(result[0].Start, _redCycles[0].Start);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "Compare Start Fail")]
        public async void CalculateVehicleArrivalsCompareStartFailTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 7:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result[0].Vehicles.Count() == 1;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "Compare End Pass")]
        public async void CalculateVehicleArrivalsCompareEndPassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result[0].Vehicles.Count() == 1;

            Assert.True(condition);

            Assert.Equal(result[0].End, _redCycles[0].End);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "Compare End Fail")]
        public async void CalculateVehicleArrivalsCompareEndFailTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 9:00:0.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var condition = result[0].Vehicles.Count() == 1;

            Assert.False(condition);
        }

        #region ICycleArrivals

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "ArrivalOnGreenPass")]
        public async void CalculateVehicleArrivalsArrivalOnGreenPassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.2") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.4") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.6") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.8") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.2") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().TotalArrivalOnGreen;
            var expected = 5;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "ArrivalOnYellowPass")]
        public async void CalculateVehicleArrivalsArrivalOnYellowPassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.2") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.4") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.6") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.8") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:3.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:3.2") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().TotalArrivalOnYellow;
            var expected = 5;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "ArrivalOnRedPass")]
        public async void CalculateVehicleArrivalsArrivalOnRedPassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.2") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.4") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.6") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.8") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.2") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().TotalArrivalOnRed;
            var expected = 5;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region ICycleVolume

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "TotalDelayPass")]
        public async void CalculateVehicleArrivalsTotalDelayPassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.2") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.4") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.6") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.8") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.2") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().TotalDelay;
            var expected = 2.5;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "TotalVolumePass")]
        public async void CalculateVehicleArrivalsTotalVolumePassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>
            {
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:0.5") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:1.5") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:2.5") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:3.0") },
                new CorrectedDetectorEvent(_detector) { CorrectedTimeStamp = DateTime.Parse("4/17/2023 8:00:3.5") }
            };

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().TotalVolume;
            var expected = 6;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region ICycle

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "TotalGreenTimePass")]
        public async void CalculateVehicleArrivalsTotalGreenTimePassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>();

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().TotalGreenTime;
            var expected = _redCycles[0].TotalGreenTime;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "TotalYellowTimePass")]
        public async void CalculateVehicleArrivalsTotalYellowTimePassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>();

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().TotalYellowTime;
            var expected = _redCycles[0].TotalYellowTime;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "TotalRedTimePass")]
        public async void CalculateVehicleArrivalsTotalRedTimePassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>();

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().TotalRedTime;
            var expected = _redCycles[0].TotalRedTime;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(CalculateVehicleArrivals), "TotalTimePass")]
        public async void CalculateVehicleArrivalsTotalTimePassTest()
        {
            var sut = new CalculateVehicleArrivals();

            var testEvents = new List<CorrectedDetectorEvent>();

            var result = await sut.ExecuteAsync(Tuple.Create<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>(testEvents, _redCycles));

            var actual = result.First().TotalTime;
            var expected = _redCycles[0].TotalTime;

            Assert.Equal(expected, actual);
        }

        #endregion

        public void Dispose()
        {
        }
    }
}
