#region license
// Copyright 2025 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests/WatchdogEmailServiceTests.cs
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

using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.NetStandardToolkit.Services;
using Xunit;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests
{
    public class WatchdogEmailServiceTests
    {
        private readonly Mock<ILogger<WatchdogEmailService>> _loggerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly WatchdogEmailService _watchdogEmailService;

        public WatchdogEmailServiceTests()
        {
            _loggerMock = new Mock<ILogger<WatchdogEmailService>>();
            _emailServiceMock = new Mock<IEmailService>();
            _watchdogEmailService = new WatchdogEmailService(_loggerMock.Object, _emailServiceMock.Object);
        }

        [Fact]
        public void GetEventsByIssueType_ShouldCategorizeEventsCorrectly()
        {
            // Arrange
            var eventsContainer = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Record count issue", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc2", DateTime.UtcNow, WatchDogComponentTypes.Location, 101, WatchDogIssueTypes.ForceOffThreshold, "Force off threshold issue", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc3", DateTime.UtcNow, WatchDogComponentTypes.Location, 102, WatchDogIssueTypes.MaxOutThreshold, "Max out threshold issue", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc4", DateTime.UtcNow, WatchDogComponentTypes.Location, 103, WatchDogIssueTypes.LowDetectorHits, "Low detector hits issue", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc5", DateTime.UtcNow, WatchDogComponentTypes.Location, 104, WatchDogIssueTypes.StuckPed, "Stuck ped issue", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc6", DateTime.UtcNow, WatchDogComponentTypes.Location, 105, WatchDogIssueTypes.UnconfiguredApproach, "Unconfigured approach issue", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc7", DateTime.UtcNow, WatchDogComponentTypes.Location, 106, WatchDogIssueTypes.UnconfiguredDetector, "Unconfigured detector issue", 1)
            };

            // Act
            WatchdogEmailService.GetEventsByIssueType(
                eventsContainer,
                out var missingErrorsLogs,
                out var forceErrorsLogs,
                out var maxErrorsLogs,
                out var countErrorsLogs,
                out var stuckpedErrorsLogs,
                out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs);

            // Assert
            Assert.Single(missingErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.RecordCount, missingErrorsLogs.First().IssueType);

            Assert.Single(forceErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.ForceOffThreshold, forceErrorsLogs.First().IssueType);

            Assert.Single(maxErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.MaxOutThreshold, maxErrorsLogs.First().IssueType);

            Assert.Single(countErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.LowDetectorHits, countErrorsLogs.First().IssueType);

            Assert.Single(stuckpedErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.StuckPed, stuckpedErrorsLogs.First().IssueType);

            Assert.Single(configurationErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.UnconfiguredApproach, configurationErrorsLogs.First().IssueType);

            Assert.Single(unconfiguredDetectorErrorsLogs);
            Assert.Equal(WatchDogIssueTypes.UnconfiguredDetector, unconfiguredDetectorErrorsLogs.First().IssueType);
        }

        [Fact]
        public void GetEventsByIssueType_ShouldHandleEmptyInput()
        {
            // Arrange
            var eventsContainer = new List<WatchDogLogEventWithCountAndDate>();

            // Act
            WatchdogEmailService.GetEventsByIssueType(
                eventsContainer,
                out var missingErrorsLogs,
                out var forceErrorsLogs,
                out var maxErrorsLogs,
                out var countErrorsLogs,
                out var stuckpedErrorsLogs,
                out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs);

            // Assert
            Assert.Empty(missingErrorsLogs);
            Assert.Empty(forceErrorsLogs);
            Assert.Empty(maxErrorsLogs);
            Assert.Empty(countErrorsLogs);
            Assert.Empty(stuckpedErrorsLogs);
            Assert.Empty(configurationErrorsLogs);
            Assert.Empty(unconfiguredDetectorErrorsLogs);
        }

        [Fact]
        public void GetEventsByIssueType_ShouldCategorizeMultipleEventsCorrectly()
        {
            // Arrange
            var eventsContainer = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Record count issue", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc2", DateTime.UtcNow, WatchDogComponentTypes.Location, 101, WatchDogIssueTypes.RecordCount, "Another record count issue", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc3", DateTime.UtcNow, WatchDogComponentTypes.Location, 102, WatchDogIssueTypes.ForceOffThreshold, "Force off threshold issue", 1),
                new WatchDogLogEventWithCountAndDate(1, "Loc4", DateTime.UtcNow, WatchDogComponentTypes.Location, 103, WatchDogIssueTypes.ForceOffThreshold, "Another force off threshold issue", 1)
            };

            // Act
            WatchdogEmailService.GetEventsByIssueType(
                eventsContainer,
                out var missingErrorsLogs,
                out var forceErrorsLogs,
                out var maxErrorsLogs,
                out var countErrorsLogs,
                out var stuckpedErrorsLogs,
                out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs);

            // Assert
            Assert.Equal(2, missingErrorsLogs.Count);
            Assert.All(missingErrorsLogs, e => Assert.Equal(WatchDogIssueTypes.RecordCount, e.IssueType));

            Assert.Equal(2, forceErrorsLogs.Count);
            Assert.All(forceErrorsLogs, e => Assert.Equal(WatchDogIssueTypes.ForceOffThreshold, e.IssueType));

            Assert.Empty(maxErrorsLogs);
            Assert.Empty(countErrorsLogs);
            Assert.Empty(stuckpedErrorsLogs);
            Assert.Empty(configurationErrorsLogs);
            Assert.Empty(unconfiguredDetectorErrorsLogs);
        }

        [Fact]
        public void GetEventsByIssueType_ShouldHandleNullContainer()
        {
            // Arrange
            List<WatchDogLogEventWithCountAndDate> eventsContainer = null;

            // Act
            WatchdogEmailService.GetEventsByIssueType(
                eventsContainer,
                out var missingErrorsLogs,
                out var forceErrorsLogs,
                out var maxErrorsLogs,
                out var countErrorsLogs,
                out var stuckpedErrorsLogs,
                out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs);

            // Assert
            Assert.Empty(missingErrorsLogs);
            Assert.Empty(forceErrorsLogs);
            Assert.Empty(maxErrorsLogs);
            Assert.Empty(countErrorsLogs);
            Assert.Empty(stuckpedErrorsLogs);
            Assert.Empty(configurationErrorsLogs);
            Assert.Empty(unconfiguredDetectorErrorsLogs);
        }

        [Fact]
        public void GetEventsByIssueType_ShouldIgnoreNullEntriesInContainer()
        {
            // Arrange
            var eventsContainer = new List<WatchDogLogEventWithCountAndDate>
    {
        null,
        new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Record count issue", 1),
        null
    };

            // Act
            WatchdogEmailService.GetEventsByIssueType(
                eventsContainer,
                out var missingErrorsLogs,
                out var forceErrorsLogs,
                out var maxErrorsLogs,
                out var countErrorsLogs,
                out var stuckpedErrorsLogs,
                out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs);

            // Assert
            Assert.Single(missingErrorsLogs);
            Assert.Empty(forceErrorsLogs);
            Assert.Empty(maxErrorsLogs);
            Assert.Empty(countErrorsLogs);
            Assert.Empty(stuckpedErrorsLogs);
            Assert.Empty(configurationErrorsLogs);
            Assert.Empty(unconfiguredDetectorErrorsLogs);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldReturnHeadersForMissingRecordsErrors()
        {
            // Arrange
            var sectionTitle = "Missing Records Errors";
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string> { "Location", "Location Description", "Issue Details", "Date of First Occurrence" };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldIncludeErrorCountsAndConsecutiveHeaders()
        {
            // Arrange
            var sectionTitle = "Missing Records Errors";
            var includeErrorCounts = true;
            var includeConsecutive = true;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string>
            {
                "Location", "Location Description", "Issue Details", "Error Count", "Consecutive Occurrence Count", "Date of First Occurrence"
            };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldReturnHeadersForForceOffErrors()
        {
            // Arrange
            var sectionTitle = "Force Off Errors";
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string> { "Location", "Location Description", "Phase", "Issue Details", "Date of First Occurrence" };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldReturnHeadersForLowDetectionCountErrors()
        {
            // Arrange
            var sectionTitle = "Low Detection Count Errors";
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string> { "Location", "Location Description", "Detector Id", "Issue Details", "Date of First Occurrence" };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldReturnDefaultHeadersForUnknownSection()
        {
            // Arrange
            var sectionTitle = "Unknown Errors";
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string> { "Location", "Location Description", "Issue Details", "Date of First Occurrence" };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldIncludeOnlyErrorCounts()
        {
            // Arrange
            var sectionTitle = "Missing Records Errors";
            var includeErrorCounts = true;
            var includeConsecutive = false;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string>
            {
                "Location", "Location Description", "Issue Details", "Error Count", "Date of First Occurrence"
            };
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public void GetTableHeadersForErrorType_ShouldIncludeOnlyConsecutiveOccurrenceCount()
        {
            // Arrange
            var sectionTitle = "Missing Records Errors";
            var includeErrorCounts = false;
            var includeConsecutive = true;

            // Act
            var headers = WatchdogEmailService.GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);

            // Assert
            var expectedHeaders = new List<string>
            {
                "Location", "Location Description", "Issue Details", "Consecutive Occurrence Count", "Date of First Occurrence"
            };
            Assert.Equal(expectedHeaders, headers);
        }

        // The following tests are for the BuildErrorSection method
        [Fact]
        public void BuildErrorSection_ShouldReturnTableWithHeadersAndRows_WhenErrorLogsArePresent()
        {
            // Arrange
            var sectionTitle = "Missing Records Errors";
            var sectionTimeDescription = "Errors detected in the last 24 hours.";
            var errorLogs = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Issue details 1", null)
                {
                    EventCount = 5,
                    ConsecutiveOccurenceCount = 3,
                    DateOfFirstInstance = DateTime.UtcNow.AddDays(-10)
                },
                new WatchDogLogEventWithCountAndDate(2, "Loc2", DateTime.UtcNow, WatchDogComponentTypes.Location, 101, WatchDogIssueTypes.RecordCount, "Issue details 2", null)
                {
                    EventCount = 10,
                    ConsecutiveOccurenceCount = 5,
                    DateOfFirstInstance = DateTime.UtcNow.AddDays(-15)
                }
            };
            var locations = new Dictionary<int, Location>
            {
                { 1, new Location { Id = 1, PrimaryName = "Main St", SecondaryName = "1st Ave", LocationIdentifier = "Loc1", Latitude = 40.1234, Longitude = -111.5678 } },
                { 2, new Location { Id = 2, PrimaryName = "Broadway", SecondaryName = "2nd Ave", LocationIdentifier = "Loc2", Latitude = 41.9876, Longitude = -112.3456 } }
            };
            var options = new WatchdogEmailOptions();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = true;
            var includeConsecutive = true;

            // Act
            var result = _watchdogEmailService.BuildErrorSection(
                sectionTitle,
                sectionTimeDescription,
                errorLogs,
                locations,
                options,
                logsFromPreviousDay,
                includeErrorCounts,
                includeConsecutive);

            // Assert
            Assert.Contains("<h3>Missing Records Errors</h3>", result);
            Assert.Contains("<p>Errors detected in the last 24 hours.</p>", result);
            Assert.Contains("<table class='atspm-table'>", result);
            Assert.Contains("<th>Location</th>", result);
            Assert.Contains("<th>Error Count</th>", result);
            Assert.Contains("<th>Consecutive Occurrence Count</th>", result);
            Assert.Contains("<td>Loc1</td>", result);
            Assert.Contains("<td>Main St & 1st Ave</td>", result);
            Assert.Contains("<td>Loc2</td>", result);
            Assert.Contains("<td>Broadway & 2nd Ave</td>", result);
        }

        [Fact]
        public void BuildErrorSection_ShouldReturnNoErrorsMessage_WhenErrorLogsAreEmpty()
        {
            // Arrange
            var sectionTitle = "Force Off Errors";
            var sectionTimeDescription = "No errors detected in the last 24 hours.";
            var errorLogs = new List<WatchDogLogEventWithCountAndDate>();
            var locations = new Dictionary<int, Location>();
            var options = new WatchdogEmailOptions();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.BuildErrorSection(
                sectionTitle,
                sectionTimeDescription,
                errorLogs,
                locations,
                options,
                logsFromPreviousDay,
                includeErrorCounts,
                includeConsecutive);

            // Assert
            Assert.Contains("<h3>Force Off Errors</h3>", result);
            Assert.Contains("<p>No errors found for this category.</p>", result);
            Assert.DoesNotContain("<table", result);
        }

        [Fact]
        public void BuildErrorSection_ShouldHandleNullErrorLogs()
        {
            // Arrange
            var sectionTitle = "Max Out Errors";
            var sectionTimeDescription = "Errors detected during the past week.";
            List<WatchDogLogEventWithCountAndDate> errorLogs = null;
            var locations = new Dictionary<int, Location>();
            var options = new WatchdogEmailOptions();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.BuildErrorSection(
                sectionTitle,
                sectionTimeDescription,
                errorLogs,
                locations,
                options,
                logsFromPreviousDay,
                includeErrorCounts,
                includeConsecutive);

            // Assert
            Assert.Contains("<h3>Max Out Errors</h3>", result);
            Assert.Contains("<p>No errors found for this category.</p>", result);
            Assert.DoesNotContain("<table", result);
        }

        [Fact]
        public void BuildErrorSection_ShouldIncludeHeadersBasedOnFlags()
        {
            // Arrange
            var sectionTitle = "Low Detection Count Errors";
            var sectionTimeDescription = "Errors detected in the past hour.";
            var errorLogs = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Detector, 100, WatchDogIssueTypes.LowDetectorHits, "Details", null)
                {
                    EventCount = 5,
                    ConsecutiveOccurenceCount = 3,
                    DateOfFirstInstance = DateTime.UtcNow.AddDays(-10)
                }
            };
            var locations = new Dictionary<int, Location>
            {
                { 1, new Location { Id = 1, PrimaryName = "Main St", SecondaryName = "1st Ave", LocationIdentifier = "Loc1", Latitude = 40.1234, Longitude = -111.5678 } }
            };
            var options = new WatchdogEmailOptions();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = true;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.BuildErrorSection(
                sectionTitle,
                sectionTimeDescription,
                errorLogs,
                locations,
                options,
                logsFromPreviousDay,
                includeErrorCounts,
                includeConsecutive);

            // Assert
            Assert.Contains("<th>Error Count</th>", result);
            Assert.DoesNotContain("<th>Consecutive Occurrence Count</th>", result);
        }

        // The following tests are for the ProcessErrorList method
        [Fact]
        public void ProcessErrorList_ShouldReturnNoErrorsMessage_WhenErrorListIsEmpty()
        {
            // Arrange
            var errorTitle = "Test Errors";
            var errorSubHeader = "No errors detected in this category.";
            var errors = new List<WatchDogLogEventWithCountAndDate>();
            var options = new WatchdogEmailOptions
            {
                ScanDate = DateTime.UtcNow
            };
            var locations = new List<Location>();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.ProcessErrorList(errorTitle, errorSubHeader, errors, options, locations, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains($"<h2>{errorTitle}</h2>", result);
            Assert.Contains($"<h4>{errorSubHeader}</h4>", result);
            Assert.Contains("<p>No errors found for this category.</p>", result);
        }

        [Fact]
        public void ProcessErrorList_ShouldHandleNullErrorList()
        {
            // Arrange
            var errorTitle = "Test Errors";
            var errorSubHeader = "No errors detected in this category.";
            List<WatchDogLogEventWithCountAndDate> errors = null;
            var options = new WatchdogEmailOptions
            {
                ScanDate = DateTime.UtcNow
            };
            var locations = new List<Location>();
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.ProcessErrorList(errorTitle, errorSubHeader, errors, options, locations, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains($"<h2>{errorTitle}</h2>", result);
            Assert.Contains($"<h4>{errorSubHeader}</h4>", result);
            Assert.Contains("<p>No errors found for this category.</p>", result);
        }

        [Fact]
        public void ProcessErrorList_ShouldIncludeCSSStyles()
        {
            // Arrange
            var errorTitle = "Test Errors";
            var errorSubHeader = "Some errors detected.";
            var errors = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Issue details", null)
            };
            var options = new WatchdogEmailOptions
            {
                ScanDate = DateTime.UtcNow
            };
            var locations = new List<Location>
            {
                new Location { Id = 1, PrimaryName = "Main St", SecondaryName = "1st Ave", LocationIdentifier = "Loc1", Latitude = 40.1234, Longitude = -111.5678 }
            };
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.ProcessErrorList(errorTitle, errorSubHeader, errors, options, locations, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains("<style>", result);
            Assert.Contains(".shaded-header", result);
            Assert.Contains(".atspm-table", result);
        }

        [Fact]
        public void ProcessErrorList_ShouldBuildSectionsForEachErrorType()
        {
            // Arrange
            var errorTitle = "Test Errors";
            var errorSubHeader = "Some errors detected.";
            var errors = new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Issue details", null),
                new WatchDogLogEventWithCountAndDate(2, "Loc2", DateTime.UtcNow, WatchDogComponentTypes.Detector, 101, WatchDogIssueTypes.LowDetectorHits, "Detector issue details", null)
            };
            var options = new WatchdogEmailOptions
            {
                ScanDate = DateTime.UtcNow,
                ScanDayStartHour = 8,
                ScanDayEndHour = 16
            };
            var locations = new List<Location>
            {
                new Location { Id = 1, PrimaryName = "Main St", SecondaryName = "1st Ave", LocationIdentifier = "Loc1", Latitude = 40.1234, Longitude = -111.5678 },
                new Location { Id = 2, PrimaryName = "Broadway", SecondaryName = "2nd Ave", LocationIdentifier = "Loc2", Latitude = 41.9876, Longitude = -112.3456 }
            };
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = true;
            var includeConsecutive = true;

            // Act
            var result = _watchdogEmailService.ProcessErrorList(errorTitle, errorSubHeader, errors, options, locations, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains("<h2 class='shaded-header'>Test Errors (Some errors detected.)</h2>", result);
            Assert.Contains("<h3>Missing Records Errors</h3>", result);
            Assert.Contains("<h3>Low Detection Count Errors</h3>", result);
            Assert.Contains("<td>Loc1</td>", result);
            Assert.Contains("<td>Main St & 1st Ave</td>", result);
            Assert.Contains("<td>Loc2</td>", result);
            Assert.Contains("<td>Broadway & 2nd Ave</td>", result);
        }

        // The following tests are for the GetMessage method
        [Fact]
        public void GetMessage_ShouldReturnEmptyString_WhenParametersAreNull()
        {
            // Arrange
            Dictionary<int, Location> locationDictionary = null;
            List<WatchDogLogEventWithCountAndDate> issues = null;
            WatchdogEmailOptions options = null;
            List<WatchDogLogEvent> logsFromPreviousDay = null;
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.GetMessage(locationDictionary, issues, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Equal(string.Empty, result);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetMessage_ShouldReturnEmptyString_WhenIssuesListIsEmpty()
        {
            // Arrange
            var locationDictionary = GetMockLocations();
            var issues = new List<WatchDogLogEventWithCountAndDate>();
            var options = new WatchdogEmailOptions { EmailAllErrors = true };
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = false;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.GetMessage(locationDictionary, issues, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetMessage_ShouldGenerateTableRows_ForValidIssues()
        {
            // Arrange
            var locationDictionary = GetMockLocations();
            var issues = GetMockWatchDogLogEvents();
            var options = new WatchdogEmailOptions { EmailAllErrors = true };
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = true;
            var includeConsecutive = true;

            // Act
            var result = _watchdogEmailService.GetMessage(locationDictionary, issues, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains("<td>Loc1</td>", result);
            Assert.Contains("<td>Main St & 1st Ave</td>", result);
            Assert.Contains("<td>5</td>", result);
            Assert.Contains("<td>3</td>", result);
            Assert.Contains("<td>2024-01-01</td>", result);
        }

        [Fact]
        public void GetMessage_ShouldExcludeIssues_FromLogsFromPreviousDay_WhenEmailAllErrorsIsFalse()
        {
            // Arrange
            var locationDictionary = GetMockLocations();
            var issues = GetMockWatchDogLogEvents();
            var options = new WatchdogEmailOptions { EmailAllErrors = false };
            var logsFromPreviousDay = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Details", null)
            };
            var includeErrorCounts = true;
            var includeConsecutive = true;

            // Act
            var result = _watchdogEmailService.GetMessage(locationDictionary, issues, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.DoesNotContain("<td>Loc1</td>", result);
            Assert.Contains("<td>Loc2</td>", result);
        }

        [Fact]
        public void GetMessage_ShouldIncludePhaseAndComponentId_WhenApplicable()
        {
            // Arrange
            var locationDictionary = GetMockLocations();
            var issues = GetMockWatchDogLogEventsWithPhaseAndComponent();
            var options = new WatchdogEmailOptions { EmailAllErrors = true };
            var logsFromPreviousDay = new List<WatchDogLogEvent>();
            var includeErrorCounts = true;
            var includeConsecutive = false;

            // Act
            var result = _watchdogEmailService.GetMessage(locationDictionary, issues, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive);

            // Assert
            Assert.Contains("<td>1</td>", result); // Phase
            Assert.Contains("<td>101</td>", result); // ComponentId
        }

        private Dictionary<int, Location> GetMockLocations()
        {
            return new Dictionary<int, Location>
            {
                { 1, new Location { Id = 1, PrimaryName = "Main St", SecondaryName = "1st Ave", LocationIdentifier = "Loc1" } },
                { 2, new Location { Id = 2, PrimaryName = "Broadway", SecondaryName = "2nd Ave", LocationIdentifier = "Loc2" } }
            };
        }

        private List<WatchDogLogEventWithCountAndDate> GetMockWatchDogLogEvents()
        {
            return new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Location, 100, WatchDogIssueTypes.RecordCount, "Details", null)
                {
                    EventCount = 5,
                    ConsecutiveOccurenceCount = 3,
                    DateOfFirstInstance = new DateTime(2024, 1, 1)
                },
                new WatchDogLogEventWithCountAndDate(2, "Loc2", DateTime.UtcNow, WatchDogComponentTypes.Location, 101, WatchDogIssueTypes.RecordCount, "Details", null)
                {
                    EventCount = 10,
                    ConsecutiveOccurenceCount = 5,
                    DateOfFirstInstance = new DateTime(2024, 1, 2)
                }
            };
        }

        private List<WatchDogLogEventWithCountAndDate> GetMockWatchDogLogEventsWithPhaseAndComponent()
        {
            return new List<WatchDogLogEventWithCountAndDate>
            {
                new WatchDogLogEventWithCountAndDate(1, "Loc1", DateTime.UtcNow, WatchDogComponentTypes.Detector, 101, WatchDogIssueTypes.LowDetectorHits, "Details", 1)
                {
                    EventCount = 5,
                    ConsecutiveOccurenceCount = 3,
                    DateOfFirstInstance = new DateTime(2024, 1, 1)
                }
            };
        }

    }
}