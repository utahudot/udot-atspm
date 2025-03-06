#region license
// Copyright 2025 Utah Departement of Transportation
// for InfrastructureTests - %Namespace%/SegmentedErrorsServiceTests.cs
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

using Xunit;
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests
{
    public class SegmentedErrorsServiceTests
    {
        [Fact]
        public void CategorizeIssues_ReturnsCorrectlyCategorizedIssues()
        {
            // Arrange
            var allConvertedRecords = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.Now, WatchDogComponentTypes.Detector, 101, WatchDogIssueTypes.RecordCount, "Details1", null) { EventCount = 0, ConsecutiveOccurenceCount = 0 }, // New issue
                new WatchDogLogEventWithCountAndDate(2, "Loc2", DateTime.Now, WatchDogComponentTypes.Approach, 102, WatchDogIssueTypes.LowDetectorHits, "Details2", 1) { EventCount = 5, ConsecutiveOccurenceCount = 0 }, // Recurring issue
                new WatchDogLogEventWithCountAndDate(3, "Loc3", DateTime.Now, WatchDogComponentTypes.Location, 103, WatchDogIssueTypes.StuckPed, "Details3", 2) { EventCount = 3, ConsecutiveOccurenceCount = 8 }, // Daily recurring
                new WatchDogLogEventWithCountAndDate(4, "Loc4", DateTime.Now, WatchDogComponentTypes.Approach, 104, WatchDogIssueTypes.ForceOffThreshold, "Details4", 3) { EventCount = 0, ConsecutiveOccurenceCount = 0 } // Should be excluded
            };


            // Act
            var (newIssues, dailyRecurringIssues, recurringIssues) = SegmentedErrorsService.CategorizeIssues(allConvertedRecords, "Location");

            // Assert
            Assert.Equal(2, newIssues.Count);
            Assert.Equal("Loc1", newIssues[0].LocationIdentifier);

            Assert.Single(dailyRecurringIssues);
            Assert.Equal("Loc3", dailyRecurringIssues[0].LocationIdentifier);

            Assert.Single(recurringIssues);
            Assert.Equal("Loc2", recurringIssues[0].LocationIdentifier);
        }


        [Fact]
        public void CategorizeIssues_NoIssues_ReturnsEmptyLists()
        {
            // Arrange
            var allConvertedRecords = new List<WatchDogLogEventWithCountAndDate>();
            var countForDayBeforeScanDate = new Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase), int>();

            // Act
            var (newIssues, dailyRecurringIssues, recurringIssues) = SegmentedErrorsService.CategorizeIssues(allConvertedRecords, "Location");

            // Assert
            Assert.Empty(newIssues);
            Assert.Empty(dailyRecurringIssues);
            Assert.Empty(recurringIssues);
        }

        [Fact]
        public void CategorizeIssues_OnlyNewIssues_ReturnsNewIssuesOnly()
        {
            // Arrange
            var allConvertedRecords = new List<WatchDogLogEventWithCountAndDate>
        {
            new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.Now, WatchDogComponentTypes.Detector, 101, WatchDogIssueTypes.MaxOutThreshold, "Details1", null) { EventCount = 0 },
            new WatchDogLogEventWithCountAndDate(2, "Loc2", DateTime.Now, WatchDogComponentTypes.Approach, 102, WatchDogIssueTypes.UnconfiguredApproach, "Details2", 1) { EventCount = 0 },
        };

            var countForDayBeforeScanDate = new Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase), int>();

            // Act
            var (newIssues, dailyRecurringIssues, recurringIssues) = SegmentedErrorsService.CategorizeIssues(allConvertedRecords, "Location");

            // Assert
            Assert.Equal(2, newIssues.Count);
            Assert.Empty(dailyRecurringIssues);
            Assert.Empty(recurringIssues);
        }

        [Fact]
        public void CategorizeIssues_OnlyRecurringIssues_ReturnsRecurringIssuesOnly()
        {
            // Arrange
            var allConvertedRecords = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.Now, WatchDogComponentTypes.Detector, 101, WatchDogIssueTypes.UnconfiguredDetector, "Details1", null) { EventCount = 5 },
                new WatchDogLogEventWithCountAndDate(2, "Loc2", DateTime.Now, WatchDogComponentTypes.Approach, 102, WatchDogIssueTypes.ForceOffThreshold, "Details2", 1) { EventCount = 3 },
                new WatchDogLogEventWithCountAndDate(3, "Loc3", DateTime.Now, WatchDogComponentTypes.Location, 103, WatchDogIssueTypes.StuckPed, "Details3", 2) { EventCount = 1, ConsecutiveOccurenceCount= 3 } // Should be excluded
            };

            // Act
            var (newIssues, dailyRecurringIssues, recurringIssues) = SegmentedErrorsService.CategorizeIssues(allConvertedRecords, "Location");

            // Assert
            Assert.Empty(newIssues);
            Assert.Single(dailyRecurringIssues);
            Assert.Equal(2, recurringIssues.Count);
            Assert.DoesNotContain(recurringIssues, r => r.LocationIdentifier == "Loc3");
        }

        [Fact]
        public void ConvertRecords_ShouldReturnCorrectlyMappedList_WhenDataExistsInLookup()
        {
            // Arrange
            var recordsForScanDate = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1),
                new WatchDogLogEvent(2, "Loc2", DateTime.UtcNow.AddMinutes(-10), WatchDogComponentTypes.Detector, 101, WatchDogIssueTypes.StuckPed, "Issue2", 2)
            };

            var countAndDateLookupForLast12Months = new Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase),
                (int Count, DateTime DateOfFirstOccurrence, int ConsecutiveOccurrenceCount)>
            {
                { ("Loc1", WatchDogIssueTypes.LowDetectorHits, WatchDogComponentTypes.Location, 1), (5, DateTime.UtcNow.AddMonths(-6), 3) },
                { ("Loc2", WatchDogIssueTypes.StuckPed, WatchDogComponentTypes.Detector, 2), (10, DateTime.UtcNow.AddMonths(-2), 5) }
            };

            // Act
            var result = SegmentedErrorsService.ConvertRecords(recordsForScanDate, countAndDateLookupForLast12Months);

            // Assert
            Assert.Equal(2, result.Count);

            var firstEvent = result.First();
            Assert.Equal(5, firstEvent.EventCount);
            Assert.Equal(DateTime.UtcNow.AddMonths(-6).Date, firstEvent.DateOfFirstInstance.Date);
            Assert.Equal(3, firstEvent.ConsecutiveOccurenceCount);

            var secondEvent = result.Last();
            Assert.Equal(10, secondEvent.EventCount);
            Assert.Equal(DateTime.UtcNow.AddMonths(-2).Date, secondEvent.DateOfFirstInstance.Date);
            Assert.Equal(5, secondEvent.ConsecutiveOccurenceCount);
        }

        [Fact]
        public void ConvertRecords_ShouldHandleMissingLookupData()
        {
            // Arrange
            var recordsForScanDate = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1)
            };

            var countAndDateLookupForLast12Months = new Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase),
                (int Count, DateTime DateOfFirstOccurrence, int ConsecutiveOccurrenceCount)>();

            // Act
            var result = SegmentedErrorsService.ConvertRecords(recordsForScanDate, countAndDateLookupForLast12Months);

            // Assert
            Assert.Single(result);

            var eventRecord = result.First();
            Assert.Equal(0, eventRecord.EventCount);
            Assert.Equal(recordsForScanDate.First().Timestamp, eventRecord.DateOfFirstInstance);
            Assert.Equal(0, eventRecord.ConsecutiveOccurenceCount);
        }

        [Fact]
        public void ConvertRecords_ShouldReturnEmptyList_WhenNoRecordsAreProvided()
        {
            // Arrange
            var recordsForScanDate = new List<WatchDogLogEvent>();
            var countAndDateLookupForLast12Months = new Dictionary<(string LocationIdentifier, WatchDogIssueTypes IssueType, WatchDogComponentTypes ComponentType, int? Phase),
                (int Count, DateTime DateOfFirstOccurrence, int ConsecutiveOccurrenceCount)>();

            // Act
            var result = SegmentedErrorsService.ConvertRecords(recordsForScanDate, countAndDateLookupForLast12Months);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void CreateCountAndDateLookup_ShouldCalculateConsecutiveOccurrences_WhenDailyRecordsStartFromPreviousDate()
        {
            // Arrange
            var recordsForLast12Months = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 1), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 2), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 3), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1)
            };
            var previousDate = new DateTime(2024, 1, 3);

            // Act
            var result = SegmentedErrorsService.CreateCountAndDateLookup(recordsForLast12Months, previousDate);

            // Assert
            var group = result[("Loc1", WatchDogIssueTypes.LowDetectorHits, WatchDogComponentTypes.Location, 1)];
            Assert.Equal(3, group.Count);
            Assert.Equal(new DateTime(2024, 1, 1), group.DateOfFirstOccurrence);
            Assert.Equal(3, group.ConsecutiveOccurrenceCount); // All 3 are consecutive.
        }

        [Fact]
        public void CreateCountAndDateLookup_ShouldBreakConsecutiveCount_WhenDatesAreNotSequential()
        {
            // Arrange
            var recordsForLast12Months = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 1), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 2), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 4), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1)
            };
            var previousDate = new DateTime(2024, 1, 4);

            // Act
            var result = SegmentedErrorsService.CreateCountAndDateLookup(recordsForLast12Months, previousDate);

            // Assert
            var group = result[("Loc1", WatchDogIssueTypes.LowDetectorHits, WatchDogComponentTypes.Location, 1)];
            Assert.Equal(3, group.Count);
            Assert.Equal(new DateTime(2024, 1, 1), group.DateOfFirstOccurrence);
            Assert.Equal(1, group.ConsecutiveOccurrenceCount); // Only the first date is consecutive with the previous date.
        }

        [Fact]
        public void CreateCountAndDateLookup_ShouldReturnZeroConsecutiveCount_WhenNoRecordsStartFromPreviousDate()
        {
            // Arrange
            var recordsForLast12Months = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 2), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 3), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1)
            };
            var previousDate = new DateTime(2024, 1, 5);

            // Act
            var result = SegmentedErrorsService.CreateCountAndDateLookup(recordsForLast12Months, previousDate);

            // Assert
            var group = result[("Loc1", WatchDogIssueTypes.LowDetectorHits, WatchDogComponentTypes.Location, 1)];
            Assert.Equal(2, group.Count);
            Assert.Equal(new DateTime(2024, 1, 2), group.DateOfFirstOccurrence);
            Assert.Equal(0, group.ConsecutiveOccurrenceCount); // No records match the day after previousDate.
        }

        [Fact]
        public void CreateCountAndDateLookup_ShouldHandleEmptyInput()
        {
            // Arrange
            var recordsForLast12Months = new List<WatchDogLogEvent>();
            var previousDate = new DateTime(2023, 12, 31);

            // Act
            var result = SegmentedErrorsService.CreateCountAndDateLookup(recordsForLast12Months, previousDate);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void CreateCountAndDateLookup_ShouldHandleSingleRecordStartingFromPreviousDate()
        {
            // Arrange
            var recordsForLast12Months = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 1), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1)
            };
            var previousDate = new DateTime(2024, 1, 1);

            // Act
            var result = SegmentedErrorsService.CreateCountAndDateLookup(recordsForLast12Months, previousDate);

            // Assert
            var group = result[("Loc1", WatchDogIssueTypes.LowDetectorHits, WatchDogComponentTypes.Location, 1)];
            Assert.Equal(1, group.Count);
            Assert.Equal(new DateTime(2024, 1, 1), group.DateOfFirstOccurrence);
            Assert.Equal(1, group.ConsecutiveOccurrenceCount); // Only one event, starting from previousDate + 1.
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_ShouldReturnZero_WhenNoEventsProvided()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>();
            var previousDay = new DateTime(2024, 1, 1);

            // Act
            var result = SegmentedErrorsService.CalculateConsecutiveOccurrences(orderedEvents, previousDay);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_ShouldReturnZero_WhenFirstEventDoesNotMatchPreviousDay()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 2), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 1), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue2", 1)
            };
            var previousDay = new DateTime(2024, 1, 3);

            // Act
            var result = SegmentedErrorsService.CalculateConsecutiveOccurrences(orderedEvents, previousDay);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_ShouldReturnCorrectStreak_WhenAllDatesAreConsecutive()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 1), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2023, 12, 31), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue2", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2023, 12, 30), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue3", 1)
            };
            var previousDay = new DateTime(2024, 1, 1);

            // Act
            var result = SegmentedErrorsService.CalculateConsecutiveOccurrences(orderedEvents, previousDay);

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_ShouldStopStreak_WhenNonConsecutiveDateIsEncountered()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 1), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2023, 12, 31), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue2", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2023, 12, 29), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue3", 1)
            };
            var previousDay = new DateTime(2024, 1, 1);

            // Act
            var result = SegmentedErrorsService.CalculateConsecutiveOccurrences(orderedEvents, previousDay);

            // Assert
            Assert.Equal(2, result); // Streak ends after 12/31.
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_ShouldHandleSingleMatchingEvent()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 1), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1)
            };
            var previousDay = new DateTime(2024, 1, 1);

            // Act
            var result = SegmentedErrorsService.CalculateConsecutiveOccurrences(orderedEvents, previousDay);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void CalculateConsecutiveOccurrences_ShouldHandleNonConsecutiveFirstEvent()
        {
            // Arrange
            var orderedEvents = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 2), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue1", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2024, 1, 1), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue2", 1),
                new WatchDogLogEvent(1, "Loc1", new DateTime(2023, 12, 31), WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.LowDetectorHits, "Issue3", 1)
            };
            var previousDay = new DateTime(2024, 1, 1);

            // Act
            var result = SegmentedErrorsService.CalculateConsecutiveOccurrences(orderedEvents, previousDay);

            // Assert
            Assert.Equal(0, result); // First event does not match previous day.
        }
    }
}