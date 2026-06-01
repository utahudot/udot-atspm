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
                new SignalTimingPlan
                {
                    LocationIdentifier = LocationIdentifier,
                    PlanNumber = 1,
                    Start = new DateTime(2026, 1, 1, 8, 0, 0),
                    End = new DateTime(2026, 1, 1, 10, 0, 0)
                },
                new SignalTimingPlan
                {
                    LocationIdentifier = "2002",
                    PlanNumber = 9,
                    Start = new DateTime(2026, 1, 1, 9, 0, 0),
                    End = new DateTime(2026, 1, 1, 11, 0, 0)
                }
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
        public void GetPlans_OpenEndedPlan_ClipsToRequestedEnd()
        {
            var start = new DateTime(2026, 1, 1, 9, 0, 0);
            var end = new DateTime(2026, 1, 1, 11, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                new SignalTimingPlan
                {
                    LocationIdentifier = LocationIdentifier,
                    PlanNumber = 254,
                    Start = new DateTime(2026, 1, 1, 8, 0, 0),
                    End = DateTime.MinValue
                }
            };

            var result = _planService.GetPlans(LocationIdentifier, start, end, rows);

            var plan = Assert.Single(result);
            Assert.Equal("254", plan.PlanNumber);
            Assert.Equal("Free", plan.PlanDescription);
            Assert.Equal(start, plan.Start);
            Assert.Equal(end, plan.End);
        }

        [Fact]
        public void GetPlans_LeadingInternalAndTrailingGaps_ReturnUnknownPlans()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 12, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                new SignalTimingPlan
                {
                    LocationIdentifier = LocationIdentifier,
                    PlanNumber = 1,
                    Start = new DateTime(2026, 1, 1, 9, 0, 0),
                    End = new DateTime(2026, 1, 1, 10, 0, 0)
                },
                new SignalTimingPlan
                {
                    LocationIdentifier = LocationIdentifier,
                    PlanNumber = 2,
                    Start = new DateTime(2026, 1, 1, 10, 30, 0),
                    End = new DateTime(2026, 1, 1, 11, 0, 0)
                }
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
        public async Task GetPlansAsync_QueriesRepositoryByLocationIdentifierAndDelegatesToReusableMethod()
        {
            var start = new DateTime(2026, 1, 1, 8, 0, 0);
            var end = new DateTime(2026, 1, 1, 10, 0, 0);
            var rows = new List<SignalTimingPlan>
            {
                new SignalTimingPlan
                {
                    LocationIdentifier = LocationIdentifier,
                    PlanNumber = 3,
                    Start = start,
                    End = end
                }
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
            Assert.True(matches(new SignalTimingPlan
            {
                LocationIdentifier = LocationIdentifier,
                PlanNumber = 1,
                Start = start.AddMinutes(15),
                End = end
            }));
            Assert.False(matches(new SignalTimingPlan
            {
                LocationIdentifier = "2002",
                PlanNumber = 1,
                Start = start.AddMinutes(15),
                End = end
            }));
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
