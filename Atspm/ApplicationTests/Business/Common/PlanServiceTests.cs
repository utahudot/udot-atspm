#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.ATSPM.ApplicationTests.Business.Common/PlanServiceTests.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.AggregationRepositories;
using Xunit;

namespace Utah.Udot.ATSPM.ApplicationTests.Business.Common
{
    public class CreatePlansFromEventsTests
    {
        private readonly PlanService _planService;

        public CreatePlansFromEventsTests()
        {
            _planService = new PlanService();
        }

        [Fact]
        public void CreatePlansFromEvents_ValidEvents_ReturnsCorrectPlans()
        {
            // Arrange
            var startDate = new DateTime(2024, 2, 1, 8, 0, 0);
            var endDate = new DateTime(2024, 2, 1, 10, 0, 0);
            var cleanedEvents = new List<IndianaEvent>
            {
                new IndianaEvent { Timestamp = new DateTime(2024, 2, 1, 8, 0, 0), EventParam = 1 },
                new IndianaEvent { Timestamp = new DateTime(2024, 2, 1, 9, 0, 0), EventParam = 2 }
            };

            // Act
            var result = _planService.GetType()
                .GetMethod("CreatePlansFromEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_planService, new object[] { cleanedEvents, startDate, endDate }) as List<Plan>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("1", result[0].PlanNumber);
            Assert.Equal(new DateTime(2024, 2, 1, 8, 0, 0), result[0].Start);
            Assert.Equal(new DateTime(2024, 2, 1, 9, 0, 0), result[0].End);
            Assert.Equal("2", result[1].PlanNumber);
            Assert.Equal(new DateTime(2024, 2, 1, 9, 0, 0), result[1].Start);
            Assert.Equal(endDate, result[1].End);
        }

        [Fact]
        public void CreatePlansFromEvents_SingleEvent_CreatesPlanToEndDate()
        {
            // Arrange
            var startDate = new DateTime(2024, 2, 1, 8, 0, 0);
            var endDate = new DateTime(2024, 2, 1, 10, 0, 0);
            var cleanedEvents = new List<IndianaEvent>
            {
                new IndianaEvent { Timestamp = startDate, EventParam = 1 }
            };

            // Act
            var result = _planService.GetType()
                .GetMethod("CreatePlansFromEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_planService, new object[] { cleanedEvents, startDate, endDate }) as List<Plan>;

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("1", result[0].PlanNumber);
            Assert.Equal(startDate, result[0].Start);
            Assert.Equal(endDate, result[0].End);
        }

        [Fact]
        public void CreatePlansFromEvents_MultipleEvents_CreatesCorrectTransitions()
        {
            // Arrange
            var startDate = new DateTime(2024, 2, 1, 8, 0, 0);
            var endDate = new DateTime(2024, 2, 1, 12, 0, 0);
            var cleanedEvents = new List<IndianaEvent>
            {
                new IndianaEvent { Timestamp = new DateTime(2024, 2, 1, 8, 0, 0), EventParam = 1 },
                new IndianaEvent { Timestamp = new DateTime(2024, 2, 1, 10, 0, 0), EventParam = 2 },
                new IndianaEvent { Timestamp = new DateTime(2024, 2, 1, 11, 0, 0), EventParam = 3 }
            };

            // Act
            var result = _planService.GetType()
                .GetMethod("CreatePlansFromEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_planService, new object[] { cleanedEvents, startDate, endDate }) as List<Plan>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(new DateTime(2024, 2, 1, 8, 0, 0), result[0].Start);
            Assert.Equal(new DateTime(2024, 2, 1, 10, 0, 0), result[0].End);
            Assert.Equal(new DateTime(2024, 2, 1, 10, 0, 0), result[1].Start);
            Assert.Equal(new DateTime(2024, 2, 1, 11, 0, 0), result[1].End);
            Assert.Equal(new DateTime(2024, 2, 1, 11, 0, 0), result[2].Start);
            Assert.Equal(endDate, result[2].End);
        }

        [Fact]
        public void CreatePlansFromEvents_EventLastingMoreThanOneDay_AdjustsEndCorrectly()
        {
            // Arrange
            var startDate = new DateTime(2024, 2, 1, 8, 0, 0);
            var endDate = new DateTime(2024, 2, 3, 10, 0, 0);
            var cleanedEvents = new List<IndianaEvent>
            {
                new IndianaEvent { Timestamp = startDate, EventParam = 1 },
                new IndianaEvent { Timestamp = new DateTime(2024, 2, 2, 12, 0, 0), EventParam = 2 }
            };

            // Act
            var result = _planService.GetType()
                .GetMethod("CreatePlansFromEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_planService, new object[] { cleanedEvents, startDate, endDate }) as List<Plan>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(startDate, result[0].Start);
            Assert.Equal(new DateTime(2024, 2, 2, 0, 0, 0), result[0].End); // Adjusted to start of next day
        }

        [Fact]
        public void CreatePlansFromEvents_EmptyEventList_ReturnsEmptyList()
        {
            // Arrange
            var startDate = new DateTime(2024, 2, 1, 8, 0, 0);
            var endDate = new DateTime(2024, 2, 1, 10, 0, 0);
            var cleanedEvents = new List<IndianaEvent>();

            // Act
            var result = _planService.GetType()
                .GetMethod("CreatePlansFromEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_planService, new object[] { cleanedEvents, startDate, endDate }) as List<Plan>;

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }


    public class PlanServiceSpeedStatisticsTests
    {
        private readonly PlanService _planService;

        public PlanServiceSpeedStatisticsTests()
        {
            _planService = new PlanService();
        }

        [Fact]
        public void SetSpeedStatistics_SingleSpeed_DoesNotThrow()
        {
            _planService.SetSpeedStatistics(
                new List<int> { 37 },
                out var avgSpeed,
                out var stdDev,
                out var eightyFifth,
                out var fifteenth);

            Assert.Equal(37, avgSpeed);
            Assert.Equal(0, stdDev);
            Assert.Equal(37, eightyFifth);
            Assert.Equal(37, fifteenth);
        }

        [Fact]
        public void SetSpeedStatistics_TwoSpeeds_DoesNotThrow()
        {
            _planService.SetSpeedStatistics(
                new List<int> { 30, 40 },
                out var avgSpeed,
                out var stdDev,
                out var eightyFifth,
                out var fifteenth);

            Assert.Equal(35, avgSpeed);
            Assert.Equal(5, stdDev);
            Assert.NotNull(eightyFifth);
            Assert.NotNull(fifteenth);
        }
    }



    public class GetTransitSignalPriorityBasicPlansTests
    {
        private readonly PlanService _planService;

        public GetTransitSignalPriorityBasicPlansTests()
        {
            _planService = new PlanService();
        }

        [Fact]
        public void GetTransitSignalPriorityBasicPlans_ValidInput_ReturnsCorrectPlans()
        {
            // Arrange
            var startDate = new DateTime(2024, 2, 1, 8, 0, 0);
            var endDate = new DateTime(2024, 2, 1, 10, 0, 0);
            var locationId = "TestLocation";
            var events = new List<IndianaEvent>
        {
            new IndianaEvent { Timestamp = new DateTime(2024, 2, 1, 8, 0, 0), EventParam = 1 },
            new IndianaEvent { Timestamp = new DateTime(2024, 2, 1, 9, 0, 0), EventParam = 2 },
            new IndianaEvent { Timestamp = new DateTime(2024, 2, 1, 10, 0, 0), EventParam = 3 }
        };

            // Act
            var result = _planService.GetTransitSignalPriorityBasicPlans(startDate, endDate, locationId, events);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("1", result[0].PlanNumber);
            Assert.Equal(startDate, result[0].Start);
            Assert.Equal(new DateTime(2024, 2, 1, 9, 0, 0), result[0].End);
        }

        [Fact]
        public void GetTransitSignalPriorityBasicPlans_StartDateGreaterThanEndDate_ThrowsException()
        {
            // Arrange
            var startDate = new DateTime(2024, 2, 2);
            var endDate = new DateTime(2024, 2, 1);
            var locationId = "TestLocation";
            var events = new List<IndianaEvent>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _planService.GetTransitSignalPriorityBasicPlans(startDate, endDate, locationId, events));
            Assert.Contains("startDate must be earlier than endDate", exception.Message);
        }

        [Fact]
        public void GetTransitSignalPriorityBasicPlans_EmptyLocationId_ThrowsException()
        {
            // Arrange
            var startDate = new DateTime(2024, 2, 1);
            var endDate = new DateTime(2024, 2, 2);
            var locationId = "";
            var events = new List<IndianaEvent>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _planService.GetTransitSignalPriorityBasicPlans(startDate, endDate, locationId, events));
            Assert.Contains("locationId cannot be null or empty", exception.Message);
        }

        [Fact]
        public void GetTransitSignalPriorityBasicPlans_EmptyEventList_ReturnsDefaultPlan()
        {
            // Arrange
            var startDate = new DateTime(2024, 2, 1, 8, 0, 0);
            var endDate = new DateTime(2024, 2, 1, 10, 0, 0);
            var locationId = "TestLocation";
            var events = new List<IndianaEvent>();

            // Act
            var result = _planService.GetTransitSignalPriorityBasicPlans(startDate, endDate, locationId, events);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(startDate, result.First().Start);
            Assert.Equal(endDate, result.First().End);
        }
    }

    public class PlanServiceSignalTimingPlanTests
    {
        private const string LocationIdentifier = "1001";
        private readonly PlanService _planService = new PlanService();

        private static SignalTimingPlan CreateSignalTimingPlan(
            string locationIdentifier,
            short planNumber,
            DateTime start,
            DateTime end,
            bool valid = true)
        {
            var signalTimingPlan = new SignalTimingPlan
            {
                LocationIdentifier = locationIdentifier,
                PlanNumber = planNumber,
                Start = start,
                End = end
            };

            typeof(SignalTimingPlan)
                .GetProperty(nameof(SignalTimingPlan.Valid))
                .SetValue(signalTimingPlan, valid);

            return signalTimingPlan;
        }

        [Fact]
        public void GetPlans_NoRows_ReturnsUnknownPlanForWindow()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 10, 0, 0);

            var result = _planService.GetPlans(LocationIdentifier, start, end, Enumerable.Empty<SignalTimingPlan>());

            var plan = Assert.Single(result);
            Assert.Equal("0", plan.PlanNumber);
            Assert.Equal("Unknown", plan.PlanDescription);
            Assert.Equal(start, plan.Start);
            Assert.Equal(end, plan.End);
        }

        [Fact]
        public void GetPlans_FiltersByLocationIdentifierAndClipsToRequestedWindow()
        {
            var start = new DateTime(2026, 1, 1, 9, 0, 0);
            var end = new DateTime(2026, 1, 1, 11, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                CreateSignalTimingPlan(
                    LocationIdentifier,
                    1,
                    new DateTime(2026, 1, 1, 8, 0, 0),
                    new DateTime(2026, 1, 1, 10, 0, 0)),
                CreateSignalTimingPlan(
                    "2002",
                    9,
                    new DateTime(2026, 1, 1, 9, 0, 0),
                    new DateTime(2026, 1, 1, 11, 0, 0))
            };

            var result = _planService.GetPlans(LocationIdentifier, start, end, rows);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("1", plan.PlanNumber);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("0", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public void GetPlans_IgnoresInvalidRows()
        {
            var start = new DateTime(2026, 1, 1, 9, 0, 0);
            var end = new DateTime(2026, 1, 1, 11, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                CreateSignalTimingPlan(
                    LocationIdentifier,
                    1,
                    start,
                    end,
                    valid: false)
            };

            var result = _planService.GetPlans(LocationIdentifier, start, end, rows);

            var plan = Assert.Single(result);
            Assert.Equal("0", plan.PlanNumber);
            Assert.Equal("Unknown", plan.PlanDescription);
            Assert.Equal(start, plan.Start);
            Assert.Equal(end, plan.End);
        }

        [Fact]
        public void GetPlans_OpenEndedPlan_ClipsToRequestedEnd()
        {
            var start = new DateTime(2026, 1, 1, 9, 0, 0);
            var end = new DateTime(2026, 1, 1, 11, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                CreateSignalTimingPlan(
                    LocationIdentifier,
                    254,
                    new DateTime(2026, 1, 1, 8, 0, 0),
                    DateTime.MinValue)
            };

            var result = _planService.GetPlans(LocationIdentifier, start, end, rows);

            var plan = Assert.Single(result);
            Assert.Equal("254", plan.PlanNumber);
            Assert.Equal("Free", plan.PlanDescription);
            Assert.Equal(start, plan.Start);
            Assert.Equal(end, plan.End);
        }

        [Fact]
        public void GetPlans_AdjacentRowsWithSamePlanNumber_AreCombined()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 11, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                CreateSignalTimingPlan(
                    LocationIdentifier,
                    7,
                    start,
                    new DateTime(2026, 1, 1, 9, 0, 0)),
                CreateSignalTimingPlan(
                    LocationIdentifier,
                    7,
                    new DateTime(2026, 1, 1, 9, 0, 0),
                    new DateTime(2026, 1, 1, 10, 0, 0)),
                CreateSignalTimingPlan(
                    LocationIdentifier,
                    8,
                    new DateTime(2026, 1, 1, 10, 0, 0),
                    end)
            };

            var result = _planService.GetPlans(LocationIdentifier, start, end, rows);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("7", plan.PlanNumber);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("8", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public void GetPlans_LeadingInternalAndTrailingGaps_ReturnUnknownPlans()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 12, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                CreateSignalTimingPlan(
                    LocationIdentifier,
                    1,
                    new DateTime(2026, 1, 1, 9, 0, 0),
                    new DateTime(2026, 1, 1, 10, 0, 0)),
                CreateSignalTimingPlan(
                    LocationIdentifier,
                    2,
                    new DateTime(2026, 1, 1, 10, 30, 0),
                    new DateTime(2026, 1, 1, 11, 0, 0))
            };

            var result = _planService.GetPlans(LocationIdentifier, start, end, rows);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("0", plan.PlanNumber);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 9, 0, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("1", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 9, 0, 0), plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("0", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 30, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("2", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 30, 0), plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 11, 0, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("0", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 11, 0, 0), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public void GetPlans_FromPlanData_ClipsAndFillsGaps()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 11, 0, 0);
            var planData = new List<Plan>
            {
                new Plan("4", new DateTime(2026, 1, 1, 7, 0, 0), new DateTime(2026, 1, 1, 9, 0, 0)),
                new Plan("5", new DateTime(2026, 1, 1, 10, 0, 0), new DateTime(2026, 1, 1, 12, 0, 0))
            };

            var result = _planService.GetPlans(start, end, planData);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("4", plan.PlanNumber);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 9, 0, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("0", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 9, 0, 0), plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("5", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public void GetPlans_FromPlanData_AdjacentSamePlanNumbersAreCombined()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 11, 0, 0);
            var planData = new List<Plan>
            {
                new Plan("4", start, new DateTime(2026, 1, 1, 9, 0, 0)),
                new Plan("4", new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0)),
                new Plan("5", new DateTime(2026, 1, 1, 10, 0, 0), end)
            };

            var result = _planService.GetPlans(start, end, planData);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("4", plan.PlanNumber);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("5", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public void GetBasicPlans_AdjacentEventsWithSamePlanNumber_AreCombined()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 11, 0, 0);
            var events = new List<IndianaEvent>
            {
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 4,
                    Timestamp = start
                },
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 4,
                    Timestamp = new DateTime(2026, 1, 1, 9, 0, 0)
                },
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 5,
                    Timestamp = new DateTime(2026, 1, 1, 10, 0, 0)
                }
            };

            var result = _planService.GetBasicPlans(start, end, LocationIdentifier, events);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("4", plan.PlanNumber);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("5", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public void GetSplitMonitorPlans_FromPlanData_ReturnsSpecializedPlanWindows()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 10, 0, 0);
            var planData = new List<Plan>
            {
                new Plan("7", start, new DateTime(2026, 1, 1, 9, 0, 0))
            };

            var result = _planService.GetSplitMonitorPlans(start, end, LocationIdentifier, planData);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("7", plan.PlanNumber);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(new DateTime(2026, 1, 1, 9, 0, 0), plan.End);
                },
                plan =>
                {
                    Assert.Equal("0", plan.PlanNumber);
                    Assert.Equal(new DateTime(2026, 1, 1, 9, 0, 0), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public async Task GetPlansAsync_QueriesRepositoryByLocationIdentifierAndDelegatesToReusableMethod()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 10, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                CreateSignalTimingPlan(LocationIdentifier, 3, start, end)
            };
            Expression<Func<SignalTimingPlan, bool>> capturedCriteria = null;
            var repository = new Mock<ISignalTimingPlanRepository>();
            repository
                .Setup(r => r.GetListAsync(It.IsAny<Expression<Func<SignalTimingPlan, bool>>>()))
                .Callback<Expression<Func<SignalTimingPlan, bool>>>(criteria => capturedCriteria = criteria)
                .ReturnsAsync(rows);

            var service = new PlanService(repository.Object);

            var result = await service.GetPlansAsync(LocationIdentifier, start, end);

            var plan = Assert.Single(result);
            Assert.Equal("3", plan.PlanNumber);
            Assert.Equal(start, plan.Start);
            Assert.Equal(end, plan.End);
            Assert.NotNull(capturedCriteria);

            var matches = capturedCriteria.Compile();
            Assert.True(matches(CreateSignalTimingPlan(
                LocationIdentifier,
                1,
                start.AddMinutes(15),
                end)));
            Assert.False(matches(CreateSignalTimingPlan(
                "2002",
                1,
                start.AddMinutes(15),
                end)));
            Assert.False(matches(CreateSignalTimingPlan(
                LocationIdentifier,
                1,
                start.AddMinutes(15),
                end,
                valid: false)));
        }

        [Fact]
        public async Task GetPlansAsync_QueryMatchesOpenEndedPlanBeforeWindowAndClipsToWindowStart()
        {
            var start = new DateTime(2026, 1, 3, 8, 0, 0);
            var end = new DateTime(2026, 1, 3, 10, 0, 0);
            var planStart = start.AddDays(-2);
            var openEndedPlan = CreateSignalTimingPlan(LocationIdentifier, 7, planStart, DateTime.MinValue);
            Expression<Func<SignalTimingPlan, bool>> capturedCriteria = null;
            var repository = new Mock<ISignalTimingPlanRepository>();
            repository
                .Setup(r => r.GetListAsync(It.IsAny<Expression<Func<SignalTimingPlan, bool>>>()))
                .Callback<Expression<Func<SignalTimingPlan, bool>>>(criteria => capturedCriteria = criteria)
                .ReturnsAsync(new List<SignalTimingPlan> { openEndedPlan });

            var service = new PlanService(repository.Object);

            var result = await service.GetPlansAsync(LocationIdentifier, start, end);

            Assert.NotNull(capturedCriteria);
            var matches = capturedCriteria.Compile();
            Assert.True(matches(openEndedPlan));
            Assert.False(matches(CreateSignalTimingPlan(
                LocationIdentifier,
                7,
                planStart,
                start.AddDays(-1))));

            var plan = Assert.Single(result);
            Assert.Equal("7", plan.PlanNumber);
            Assert.Equal("Plan 7", plan.PlanDescription);
            Assert.Equal(start, plan.Start);
            Assert.Equal(end, plan.End);
        }

        [Fact]
        public async Task GetPlansAsync_WithFallbackEvents_UsesTableRowsWhenPresent()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 10, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                CreateSignalTimingPlan(LocationIdentifier, 3, start, end)
            };
            var fallbackEvents = new List<IndianaEvent>
            {
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 7,
                    Timestamp = start
                }
            };
            var repository = new Mock<ISignalTimingPlanRepository>();
            repository
                .Setup(r => r.GetListAsync(It.IsAny<Expression<Func<SignalTimingPlan, bool>>>()))
                .ReturnsAsync(rows);

            var service = new PlanService(repository.Object);

            var result = await service.GetPlansAsync(LocationIdentifier, start, end, fallbackEvents);

            var plan = Assert.Single(result);
            Assert.Equal("3", plan.PlanNumber);
            Assert.Equal(start, plan.Start);
            Assert.Equal(end, plan.End);
        }

        [Fact]
        public async Task GetPlansAsync_WithFallbackEvents_UsesFallbackWhenOnlyTableRowsAreInvalid()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 10, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                CreateSignalTimingPlan(
                    LocationIdentifier,
                    3,
                    start,
                    end,
                    valid: false)
            };
            var fallbackEvents = new List<IndianaEvent>
            {
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 7,
                    Timestamp = start.AddHours(1)
                }
            };
            var repository = new Mock<ISignalTimingPlanRepository>();
            repository
                .Setup(r => r.GetListAsync(It.IsAny<Expression<Func<SignalTimingPlan, bool>>>()))
                .ReturnsAsync(rows);

            var service = new PlanService(repository.Object);

            var result = await service.GetPlansAsync(LocationIdentifier, start, end, fallbackEvents);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("0", plan.PlanNumber);
                    Assert.Equal("Unknown", plan.PlanDescription);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(start.AddHours(1), plan.End);
                },
                plan =>
                {
                    Assert.Equal("7", plan.PlanNumber);
                    Assert.Equal("Plan 7", plan.PlanDescription);
                    Assert.Equal(start.AddHours(1), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public async Task GetPlansAsync_WithFallbackEvents_UsesFallbackWhenSingleOpenEndedTablePlanIsStaleAndEventsChangePlan()
        {
            var start = new DateTime(2026, 1, 3, 8, 0, 0);
            var end = new DateTime(2026, 1, 3, 10, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                CreateSignalTimingPlan(LocationIdentifier, 7, start.AddDays(-2), DateTime.MinValue)
            };
            var fallbackEvents = new List<IndianaEvent>
            {
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 7,
                    Timestamp = start.AddMinutes(-5)
                },
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 8,
                    Timestamp = start.AddHours(1)
                }
            };
            var repository = new Mock<ISignalTimingPlanRepository>();
            repository
                .Setup(r => r.GetListAsync(It.IsAny<Expression<Func<SignalTimingPlan, bool>>>()))
                .ReturnsAsync(rows);

            var service = new PlanService(repository.Object);

            var result = await service.GetPlansAsync(LocationIdentifier, start, end, fallbackEvents);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("7", plan.PlanNumber);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(start.AddHours(1), plan.End);
                },
                plan =>
                {
                    Assert.Equal("8", plan.PlanNumber);
                    Assert.Equal(start.AddHours(1), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public async Task GetPlansAsync_WithFallbackEvents_KeepsSingleOpenEndedTablePlanWhenEventsDoNotChangePlan()
        {
            var start = new DateTime(2026, 1, 3, 8, 0, 0);
            var end = new DateTime(2026, 1, 3, 10, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                CreateSignalTimingPlan(LocationIdentifier, 7, start.AddDays(-2), DateTime.MinValue)
            };
            var fallbackEvents = new List<IndianaEvent>
            {
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 7,
                    Timestamp = start.AddHours(1)
                }
            };
            var repository = new Mock<ISignalTimingPlanRepository>();
            repository
                .Setup(r => r.GetListAsync(It.IsAny<Expression<Func<SignalTimingPlan, bool>>>()))
                .ReturnsAsync(rows);

            var service = new PlanService(repository.Object);

            var result = await service.GetPlansAsync(LocationIdentifier, start, end, fallbackEvents);

            var plan = Assert.Single(result);
            Assert.Equal("7", plan.PlanNumber);
            Assert.Equal(start, plan.Start);
            Assert.Equal(end, plan.End);
        }

        [Fact]
        public async Task GetPlansAsync_WithFallbackEvents_UsesProvidedPlanEventsWhenNoTableRows()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 10, 0, 0);
            var fallbackEvents = new List<IndianaEvent>
            {
                new IndianaEvent
                {
                    LocationIdentifier = "2002",
                    EventCode = 131,
                    EventParam = 9,
                    Timestamp = start
                },
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 1,
                    EventParam = 8,
                    Timestamp = start
                },
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 4,
                    Timestamp = start
                },
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 5,
                    Timestamp = start.AddHours(1)
                }
            };
            var repository = new Mock<ISignalTimingPlanRepository>();
            repository
                .Setup(r => r.GetListAsync(It.IsAny<Expression<Func<SignalTimingPlan, bool>>>()))
                .ReturnsAsync(new List<SignalTimingPlan>());

            var service = new PlanService(repository.Object);

            var result = await service.GetPlansAsync(LocationIdentifier, start, end, fallbackEvents);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("4", plan.PlanNumber);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(start.AddHours(1), plan.End);
                },
                plan =>
                {
                    Assert.Equal("5", plan.PlanNumber);
                    Assert.Equal(start.AddHours(1), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public async Task GetPlansAsync_WithFallbackEvents_DoesNotMutateProvidedEvents()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 10, 0, 0);
            var originalTimestamp = start.AddMinutes(-15);
            var fallbackEvent = new IndianaEvent
            {
                LocationIdentifier = LocationIdentifier,
                EventCode = 131,
                EventParam = 7,
                Timestamp = originalTimestamp
            };
            var repository = new Mock<ISignalTimingPlanRepository>();
            repository
                .Setup(r => r.GetListAsync(It.IsAny<Expression<Func<SignalTimingPlan, bool>>>()))
                .ReturnsAsync(new List<SignalTimingPlan>());

            var service = new PlanService(repository.Object);

            var result = await service.GetPlansAsync(LocationIdentifier, start, end, new List<IndianaEvent> { fallbackEvent });

            Assert.Equal(originalTimestamp, fallbackEvent.Timestamp);
            var plan = Assert.Single(result);
            Assert.Equal("7", plan.PlanNumber);
            Assert.Equal(start, plan.Start);
            Assert.Equal(end, plan.End);
        }

        [Fact]
        public async Task GetPlansAsync_WithFallbackEvents_UsesSuppliedPreWindowEventForStartingPlan()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 10, 0, 0);
            var fallbackEvents = new List<IndianaEvent>
            {
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 7,
                    Timestamp = start.AddMinutes(-1)
                },
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 8,
                    Timestamp = start.AddHours(1)
                },
                new IndianaEvent
                {
                    LocationIdentifier = LocationIdentifier,
                    EventCode = 131,
                    EventParam = 9,
                    Timestamp = end
                }
            };
            var repository = new Mock<ISignalTimingPlanRepository>();
            repository
                .Setup(r => r.GetListAsync(It.IsAny<Expression<Func<SignalTimingPlan, bool>>>()))
                .ReturnsAsync(new List<SignalTimingPlan>());

            var service = new PlanService(repository.Object);

            var result = await service.GetPlansAsync(LocationIdentifier, start, end, fallbackEvents);

            Assert.Collection(
                result,
                plan =>
                {
                    Assert.Equal("7", plan.PlanNumber);
                    Assert.Equal(start, plan.Start);
                    Assert.Equal(start.AddHours(1), plan.End);
                },
                plan =>
                {
                    Assert.Equal("8", plan.PlanNumber);
                    Assert.Equal(start.AddHours(1), plan.Start);
                    Assert.Equal(end, plan.End);
                });
        }

        [Fact]
        public async Task GetPlansAsync_WithoutRepository_ThrowsInvalidOperationException()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 10, 0, 0);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _planService.GetPlansAsync(LocationIdentifier, start, end));
        }
    }
}
