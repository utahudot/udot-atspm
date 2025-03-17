using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

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
}