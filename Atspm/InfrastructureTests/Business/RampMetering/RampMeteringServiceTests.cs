#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.InfrastructureTests.Business.RampMetering/RampMeteringServiceTests.cs
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

#nullable enable
#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.InfrastructureTests.Business.RampMetering/RampMeteringServiceTests.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Utah.Udot.Atspm.Business.RampMetering;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Xunit;

namespace Utah.Udot.ATSPM.InfrastructureTests.Business.RampMetering
{
    public class RampMeteringServiceTests
    {
        private readonly RampMeteringService service = new();

        [Fact]
        public void Constructor_Should_Create_Instance()
        {
            Assert.NotNull(service);
        }

        [Fact]
        public void GetChartData_Should_MapFlowOccupancyAndSpeed_WithinWindow()
        {
            var location = new Location
            {
                LocationIdentifier = "2725"
            };

            var start = new DateTime(2026, 4, 24, 15, 0, 0);
            var end = new DateTime(2026, 4, 24, 19, 0, 0);
            var options = new RampMeteringOptions
            {
                Start = start,
                End = end,
                CombineLanes = false
            };

            var events = new List<IndianaEvent>
            {
                new()
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = start.AddMinutes(-1),
                    EventCode = 1371,
                    EventParam = 999
                },
                new()
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = start.AddMinutes(1),
                    EventCode = 1371,
                    EventParam = 1700
                },
                new()
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = start.AddMinutes(1),
                    EventCode = 1372,
                    EventParam = 118
                },
                new()
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = start.AddMinutes(1),
                    EventCode = 1373,
                    EventParam = 116
                },
                new()
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = end.AddMinutes(1),
                    EventCode = 1371,
                    EventParam = 888
                }
            };

            var result = service.GetChartData(location, options, events);

            Assert.NotNull(result);
            Assert.Single(result.MainlineAvgFlow);
            Assert.Single(result.MainlineAvgOcc);
            Assert.Single(result.MainlineAvgSpeed);

            Assert.Equal(1700, result.MainlineAvgFlow[0].Value);
            Assert.Equal(11, result.MainlineAvgOcc[0].Value);
            Assert.Equal(116 * 0.621371, result.MainlineAvgSpeed[0].Value, 6);

            Assert.Equal(start.AddMinutes(1), result.MainlineAvgFlow[0].Timestamp);
            Assert.Equal(start.AddMinutes(1), result.MainlineAvgOcc[0].Timestamp);
            Assert.Equal(start.AddMinutes(1), result.MainlineAvgSpeed[0].Timestamp);
        }

        [Fact]
        public void GetChartData_Should_RespectCombineLanes_ForRateSeries()
        {
            var location = new Location
            {
                LocationIdentifier = "2725"
            };

            var options = new RampMeteringOptions
            {
                Start = new DateTime(2026, 4, 24, 15, 0, 0),
                End = new DateTime(2026, 4, 24, 19, 0, 0),
                CombineLanes = true
            };

            var events = new List<IndianaEvent>
            {
                new()
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = options.Start.AddMinutes(1),
                    EventCode = 1058,
                    EventParam = 1
                },
                new()
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = options.Start.AddMinutes(2),
                    EventCode = 1059,
                    EventParam = 2
                },
                new()
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = options.Start.AddMinutes(3),
                    EventCode = 1042,
                    EventParam = 3
                },
                new()
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = options.Start.AddMinutes(4),
                    EventCode = 1043,
                    EventParam = 4
                }
            };

            var result = service.GetChartData(location, options, events);

            Assert.Single(result.LanesActiveRate);
            Assert.Single(result.LanesBaseRate);
            Assert.Equal("1", result.LanesActiveRate[0].Description);
            Assert.Equal("1", result.LanesBaseRate[0].Description);
            Assert.Single(result.LanesActiveRate[0].Value);
            Assert.Single(result.LanesBaseRate[0].Value);
            Assert.Equal(1, result.LanesActiveRate[0].Value[0].Value);
            Assert.Equal(3, result.LanesBaseRate[0].Value[0].Value);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetChartData_Should_KeepLaneIdentity_WhenStartTimeChanges(bool combineLanes)
        {
            var location = new Location { LocationIdentifier = "2309" };
            var start = new DateTime(2026, 10, 6, 6, 0, 0);
            var end = start.AddHours(3);
            var events = new List<IndianaEvent>
            {
                RateEvent(start.AddMinutes(45), 1058, 510),
                RateEvent(start.AddMinutes(45), 1059, 620),
                RateEvent(start.AddMinutes(45), 1060, 730),
                RateEvent(start.AddMinutes(45), 1061, 840),
                RateEvent(start.AddHours(1).AddSeconds(1), 1059, 621),
                RateEvent(start.AddHours(1).AddSeconds(2), 1060, 731),
                RateEvent(start.AddHours(1).AddSeconds(3), 1061, 841),
                RateEvent(start.AddHours(1).AddSeconds(4), 1058, 511),
                RateEvent(start.AddMinutes(105), 1044, 700),
                RateEvent(start.AddMinutes(105), 1042, 500),
                RateEvent(start.AddMinutes(105), 1045, 800),
                RateEvent(start.AddMinutes(105), 1043, 600)
            };
            var laterStart = start.AddHours(1);

            var fullWindow = service.GetChartData(location, new RampMeteringOptions
            {
                Start = start,
                End = end,
                CombineLanes = combineLanes
            }, events);
            var laterWindow = service.GetChartData(location, new RampMeteringOptions
            {
                Start = laterStart,
                End = end,
                CombineLanes = combineLanes
            }, events);

            var expectedLanes = combineLanes ? new[] { "1" } : new[] { "1", "2", "3", "4" };
            var expectedActiveRates = combineLanes ? new double[] { 511 } : new double[] { 511, 621, 731, 841 };
            var expectedBaseRates = combineLanes ? new double[] { 500 } : new double[] { 500, 600, 700, 800 };

            foreach (var result in new[] { fullWindow, laterWindow })
            {
                Assert.Equal(expectedLanes, result.LanesActiveRate.Select(lane => lane.Description));
                Assert.Equal(expectedLanes, result.LanesBaseRate.Select(lane => lane.Description));
                Assert.Equal(expectedBaseRates, result.LanesBaseRate.Select(lane => Assert.Single(lane.Value).Value));
            }

            Assert.Equal(expectedActiveRates, laterWindow.LanesActiveRate.Select(lane => Assert.Single(lane.Value).Value));
            foreach (var lane in laterWindow.LanesActiveRate)
            {
                var originalLane = fullWindow.LanesActiveRate.Single(original => original.Description == lane.Description);
                Assert.Equal(
                    originalLane.Value.Where(point => point.Timestamp >= laterStart).Select(point => (point.Timestamp, point.Value)),
                    lane.Value.Select(point => (point.Timestamp, point.Value)));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetChartData_Should_PreserveLaneNumbers_WhenRateTypesHaveDifferentLanes(bool combineLanes)
        {
            var location = new Location { LocationIdentifier = "2309" };
            var start = new DateTime(2026, 10, 6, 7, 0, 0);
            var options = new RampMeteringOptions
            {
                Start = start,
                End = start.AddHours(2),
                CombineLanes = combineLanes
            };
            var events = new List<IndianaEvent>
            {
                RateEvent(start.AddSeconds(1), 1061, 840),
                RateEvent(start.AddSeconds(2), 1059, 620),
                RateEvent(start.AddSeconds(3), 1058, 510),
                RateEvent(start.AddMinutes(45), 1044, 700),
                RateEvent(start.AddMinutes(45), 1045, 800),
                RateEvent(start.AddMinutes(45), 1043, 600)
            };

            var result = service.GetChartData(location, options, events);

            var expectedActiveLanes = combineLanes ? new[] { "1" } : new[] { "1", "2", "4" };
            var expectedBaseLanes = combineLanes ? Array.Empty<string>() : new[] { "2", "3", "4" };
            var expectedActiveRates = combineLanes ? new double[] { 510 } : new double[] { 510, 620, 840 };
            var expectedBaseRates = combineLanes ? Array.Empty<double>() : new double[] { 600, 700, 800 };

            Assert.Equal(expectedActiveLanes, result.LanesActiveRate.Select(lane => lane.Description));
            Assert.Equal(expectedBaseLanes, result.LanesBaseRate.Select(lane => lane.Description));
            Assert.Equal(expectedActiveRates, result.LanesActiveRate.Select(lane => Assert.Single(lane.Value).Value));
            Assert.Equal(expectedBaseRates, result.LanesBaseRate.Select(lane => Assert.Single(lane.Value).Value));
        }

        private static IndianaEvent RateEvent(DateTime timestamp, short eventCode, short rate) => new()
        {
            LocationIdentifier = "2309",
            Timestamp = timestamp,
            EventCode = eventCode,
            EventParam = rate
        };
    }
}
