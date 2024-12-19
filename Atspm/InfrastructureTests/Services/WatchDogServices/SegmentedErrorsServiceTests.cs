using Xunit;
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests
{
    public class SegmentedErrorsServiceTests
    {

        [Fact]
        public void CalculateConsecutiveOccurrences_ShouldReturnCorrectStreak()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 10), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details1", null),
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 11), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details2", null),
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 12), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details3", null), // 3-day streak
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 14), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details4", null), // Gap
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 15), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details5", null)  // 2-day streak
            };

            // Act
            int result = SegmentedErrorsService.CalculateLastConsecutiveOccurrences(orderedEvents);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_EmptyList_ShouldReturnZero()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>();

            // Act
            int result = SegmentedErrorsService.CalculateLastConsecutiveOccurrences(orderedEvents);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_SingleEvent_ShouldReturnOne()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 10), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details1", null)
            };

            // Act
            int result = SegmentedErrorsService.CalculateLastConsecutiveOccurrences(orderedEvents);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_NoConsecutiveDays_ShouldReturnOne()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 10), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details1", null),
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 12), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details2", null),
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 14), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details3", null)
            };

            // Act
            int result = SegmentedErrorsService.CalculateLastConsecutiveOccurrences(orderedEvents);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_AllDaysConsecutive_ShouldReturnFullCount()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 10), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details1", null),
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 11), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details2", null),
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 12), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details3", null),
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 13), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details4", null),
                new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 14), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details5", null)
            };

            // Act
            int result = SegmentedErrorsService.CalculateLastConsecutiveOccurrences(orderedEvents);

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_MultipleGroupings_ShouldReturnLongestStreak()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>
    {
        new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 10), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details1", null),
        new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 11), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details2", null), // 2-day streak
        new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 13), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details3", null),
        new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 14), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details4", null),
        new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 15), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details5", null), // 3-day streak
        new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 20), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details6", null),
        new WatchDogLogEvent(1, "Location1", new DateTime(2024, 12, 21), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, "Details7", null)  // 2-day streak
    };

            // Act
            int result = SegmentedErrorsService.CalculateLastConsecutiveOccurrences(orderedEvents);

            // Assert
            Assert.Equal(2, result);
        }
        [Fact]
        public void CreateCountAndDateLookup_EmptyList_ReturnsEmptyDictionary()
        {
            // Arrange
            var records = new List<WatchDogLogEvent>();

            // Act
            var result = SegmentedErrorsService.CreateCountAndDateLookup(records);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void CreateCountAndDateLookup_SingleRecord_ReturnsCorrectData()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var records = new List<WatchDogLogEvent>
        {
            new WatchDogLogEvent(
                locationId: 1,
                locationIdentifier: "Location1",
                timestamp: timestamp,
                componentType: WatchDogComponentTypes.Location,
                componentId: 1,
                issueType: WatchDogIssueTypes.LowDetectorHits,
                details: "Details",
                phase: 2)
        };

            // Act
            var result = SegmentedErrorsService.CreateCountAndDateLookup(records);

            // Assert
            Assert.Single(result);
            var key = ("Location1", WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, 2);
            Assert.True(result.ContainsKey(key));
            var value = result[key];
            Assert.Equal(1, value.Count);
            Assert.Equal(timestamp, value.DateOfFirstOccurrence);
            Assert.Equal(1, value.ConsecutiveOccurrenceCount);
        }

        [Fact]
        public void CreateCountAndDateLookup_MultipleRecordsGroupedCorrectly()
        {
            // Arrange
            var timestamp1 = DateTime.UtcNow.AddDays(-2);
            var timestamp2 = timestamp1.AddDays(1);
            var records = new List<WatchDogLogEvent>
        {
            new WatchDogLogEvent(
                locationId: 1,
                locationIdentifier: "Location1",
                timestamp: timestamp1,
                componentType: WatchDogComponentTypes.Location,
                componentId: 1,
                issueType: WatchDogIssueTypes.LowDetectorHits,
                details: "Details1",
                phase: 2),
            new WatchDogLogEvent(
                locationId: 1,
                locationIdentifier: "Location1",
                timestamp: timestamp2,
                componentType: WatchDogComponentTypes.Location,
                componentId: 1,
                issueType: WatchDogIssueTypes.LowDetectorHits,
                details: "Details2",
                phase: 2)
        };

            // Act
            var result = SegmentedErrorsService.CreateCountAndDateLookup(records);

            // Assert
            Assert.Single(result);
            var key = ("Location1", WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, 2);
            Assert.True(result.ContainsKey(key));
            var value = result[key];
            Assert.Equal(2, value.Count);
            Assert.Equal(timestamp1, value.DateOfFirstOccurrence);
            Assert.Equal(2, value.ConsecutiveOccurrenceCount); // Assuming CalculateLastConsecutiveOccurrences works as expected.
        }

        [Fact]
        public void CreateCountAndDateLookup_DistinctGroups_ReturnsCorrectDataForEachGroup()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var records = new List<WatchDogLogEvent>
        {
            new WatchDogLogEvent(
                locationId: 1,
                locationIdentifier: "Location1",
                timestamp: timestamp,
                componentType: WatchDogComponentTypes.Location,
                componentId: 1,
                issueType: WatchDogIssueTypes.LowDetectorHits,
                details: "Details1",
                phase: 2),
            new WatchDogLogEvent(
                locationId: 2,
                locationIdentifier: "Location2",
                timestamp: timestamp,
                componentType: WatchDogComponentTypes.Approach,
                componentId: 2,
                issueType: WatchDogIssueTypes.MaxOutThreshold,
                details: "Details2",
                phase: 3)
        };

            // Act
            var result = SegmentedErrorsService.CreateCountAndDateLookup(records);

            // Assert
            Assert.Equal(2, result.Count);

            var key1 = ("Location1", WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, 2);
            var key2 = ("Location2", WatchDogComponentTypes.Approach, 2, WatchDogIssueTypes.MaxOutThreshold, 3);

            Assert.True(result.ContainsKey(key1));
            Assert.True(result.ContainsKey(key2));

            Assert.Equal(1, result[key1].Count);
            Assert.Equal(timestamp, result[key1].DateOfFirstOccurrence);
            Assert.Equal(1, result[key1].ConsecutiveOccurrenceCount);

            Assert.Equal(1, result[key2].Count);
            Assert.Equal(timestamp, result[key2].DateOfFirstOccurrence);
            Assert.Equal(1, result[key2].ConsecutiveOccurrenceCount);
        }

        [Fact]
        public void CreateCountAndDateLookup_HandlesConsecutiveOccurrencesCorrectly()
        {
            // Arrange
            var timestamp1 = DateTime.UtcNow.AddDays(-3);
            var timestamp2 = timestamp1.AddDays(1);
            var timestamp3 = timestamp2.AddDays(1);
            var records = new List<WatchDogLogEvent>
        {
            new WatchDogLogEvent(
                locationId: 1,
                locationIdentifier: "Location1",
                timestamp: timestamp1,
                componentType: WatchDogComponentTypes.Location,
                componentId: 1,
                issueType: WatchDogIssueTypes.LowDetectorHits,
                details: "Details1",
                phase: null),
            new WatchDogLogEvent(
                locationId: 1,
                locationIdentifier: "Location1",
                timestamp: timestamp2,
                componentType: WatchDogComponentTypes.Location,
                componentId: 1,
                issueType: WatchDogIssueTypes.LowDetectorHits,
                details: "Details2",
                phase: null),
            new WatchDogLogEvent(
                locationId: 1,
                locationIdentifier: "Location1",
                timestamp: timestamp3,
                componentType: WatchDogComponentTypes.Location,
                componentId: 1,
                issueType: WatchDogIssueTypes.LowDetectorHits,
                details: "Details3",
                phase: null)
        };

            // Act
            var result = SegmentedErrorsService.CreateCountAndDateLookup(records);

            // Assert
            var key = ("Location1", WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.LowDetectorHits, (int?)null);
            Assert.True(result.ContainsKey(key));
            var value = result[key];
            Assert.Equal(3, value.Count);
            Assert.Equal(timestamp1, value.DateOfFirstOccurrence);
            Assert.Equal(3, value.ConsecutiveOccurrenceCount); // Assuming CalculateLastConsecutiveOccurrences is correct.
        }

    }
    }
