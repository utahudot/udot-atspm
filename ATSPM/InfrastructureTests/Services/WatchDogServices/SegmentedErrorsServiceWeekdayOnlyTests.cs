#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.InfrastructureTests.Services.WatchDogServices/SegmentedErrorsServiceWeekdayOnlyTests.cs
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
using System.Linq.Expressions;
using Moq;
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories;
using Xunit;

namespace Utah.Udot.ATSPM.InfrastructureTests.Services.WatchDogServices
{
    public class SegmentedErrorsServiceWeekdayOnlyTests
    {
        [Fact]
        public void GetSegmentedErrors_WeekdayOnly_OnMondayUsesFridayAsPreviousDay()
        {
            // Arrange
            var scanDate = new DateTime(2024, 1, 8); // Monday
            var scanRecords = new List<WatchDogLogEvent>
            {
                CreateEvent(1, "Loc1", scanDate, WatchDogIssueTypes.LowDetectorHits)
            };

            var historicalEvents = new List<WatchDogLogEvent>
            {
                CreateEvent(1, "Loc1", new DateTime(2024, 1, 4), WatchDogIssueTypes.LowDetectorHits), // Thursday
                CreateEvent(1, "Loc1", new DateTime(2024, 1, 5), WatchDogIssueTypes.LowDetectorHits)  // Friday
            };

            var service = new SegmentedErrorsService(CreateRepository(historicalEvents).Object);

            // Act
            var (newIssues, dailyRecurringIssues, recurringIssues) =
                service.GetSegmentedErrors(scanRecords, weekdayOnly: true, "Location", scanDate);

            // Assert
            Assert.Empty(newIssues);
            Assert.Single(dailyRecurringIssues);
            Assert.Equal("Loc1", dailyRecurringIssues[0].LocationIdentifier);
            Assert.Empty(recurringIssues);
        }

        [Fact]
        public void GetSegmentedErrors_WithoutWeekdayOnly_OnMondayUsesSundayAsPreviousDay()
        {
            // Arrange
            var scanDate = new DateTime(2024, 1, 8); // Monday
            var scanRecords = new List<WatchDogLogEvent>
            {
                CreateEvent(1, "Loc1", scanDate, WatchDogIssueTypes.LowDetectorHits)
            };

            var historicalEvents = new List<WatchDogLogEvent>
            {
                CreateEvent(1, "Loc1", new DateTime(2024, 1, 6), WatchDogIssueTypes.LowDetectorHits), // Saturday
                CreateEvent(1, "Loc1", new DateTime(2024, 1, 7), WatchDogIssueTypes.LowDetectorHits)  // Sunday
            };

            var service = new SegmentedErrorsService(CreateRepository(historicalEvents).Object);

            // Act
            var (newIssues, dailyRecurringIssues, recurringIssues) =
                service.GetSegmentedErrors(scanRecords, weekdayOnly: false, "Location", scanDate);

            // Assert
            Assert.Empty(newIssues);
            Assert.Single(dailyRecurringIssues);
            Assert.Equal("Loc1", dailyRecurringIssues[0].LocationIdentifier);
            Assert.Empty(recurringIssues);
        }

        private static Mock<IWatchDogEventLogRepository> CreateRepository(IEnumerable<WatchDogLogEvent> events)
        {
            var repository = new Mock<IWatchDogEventLogRepository>();

            repository
                .Setup(r => r.GetList(It.IsAny<Expression<Func<WatchDogLogEvent, bool>>>()))
                .Returns((Expression<Func<WatchDogLogEvent, bool>> predicate) => events.Where(predicate.Compile()).ToList());

            return repository;
        }

        private static WatchDogLogEvent CreateEvent(int locationId, string locationIdentifier, DateTime timestamp, WatchDogIssueTypes issueType)
        {
            return new WatchDogLogEvent(
                locationId,
                locationIdentifier,
                timestamp,
                WatchDogComponentTypes.Location,
                100,
                issueType,
                "test",
                "1",
                1);
        }
    }
}
