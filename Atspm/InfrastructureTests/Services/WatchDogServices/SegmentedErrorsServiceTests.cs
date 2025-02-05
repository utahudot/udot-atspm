using Xunit;
using System;
using System.Collections.Generic;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Utah.Udot.Atspm.Infrastructure.Services.EmailServices;
using Utah.Udot.NetStandardToolkit.Configuration;
using Utah.Udot.Atspm.Repositories;
using System.Linq.Expressions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Microsoft.AspNetCore.Identity;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using System.Linq;
using System.Threading;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Services;

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


        [Fact()]
        public async void CalculateDailyRecurringIssues_EmptyList_ReturnsEmptyList()
        {
            var loggerMock = new Mock<ILogger<WatchdogEmailService>>();
            var smtpLoggerMock = new Mock<ILogger<SmtpEmailService>>();
            var configurationMock = new Mock<IOptionsSnapshot<EmailConfiguration>>();
            var mailMock = new SmtpEmailService(configurationMock.Object, smtpLoggerMock.Object);
            var emailService = new WatchdogEmailService(loggerMock.Object, mailMock);
            var emailOptions = new WatchdogEmailOptions
            {
                PreviousDayPMPeakEnd = 17,
                PreviousDayPMPeakStart = 18,
                ScanDate = new DateTime(2023, 8, 24),
                ScanDayEndHour = 5,
                ScanDayStartHour = 1,
                WeekdayOnly = false,
                DefaultEmailAddress = "derekjlowe@gmail.com",
                EmailAllErrors = true
            };

            var scanDateLogs = new List<WatchDogLogEvent> {
                new WatchDogLogEvent(1, "1001", new DateTime(2023, 8, 24), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.RecordCount, "Details 1", null),
            };
            var dayBeforeScanDate = new List<WatchDogLogEvent>();
            var last12MonthsLogs = new List<WatchDogLogEvent> {
                new WatchDogLogEvent(1, "1001", new DateTime(2023, 7, 24), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.RecordCount, "Details 1", null),
            };

            var mockWatchdogLogRepository = new Mock<IWatchDogEventLogRepository>();
            mockWatchdogLogRepository
                .SetupSequence(repo => repo.GetList(It.IsAny<Expression<Func<WatchDogLogEvent, bool>>>()))
                .Returns(dayBeforeScanDate)
                .Returns(last12MonthsLogs);

            var segmentedErrorService = new SegmentedErrorsService(mockWatchdogLogRepository.Object);
            var result = segmentedErrorService.GetSegmentedErrors(scanDateLogs, emailOptions);

            Assert.Empty(result.dailyRecurringIssues);
        }

        [Fact]
        public async void StartScan_WeekdayOnlyTrue_ShouldNotCreateEmail()
        {
            // Arrange
            var mockDependencies = SetupMocks();
            var emailOptions = new WatchdogEmailOptions
            {
                WeekdayOnly = true,
                ScanDate = new DateTime(2024, 12, 14) // Saturday
            };
            var loggingOptions = new WatchdogLoggingOptions();
            var cancellationToken = new CancellationToken();

            // Act
            await mockDependencies.ScanService.StartScan(loggingOptions, emailOptions, cancellationToken);

            // Assert
            mockDependencies.EmailServiceMock.Verify(service => service.SendAllEmails(
                It.IsAny<WatchdogEmailOptions>(),
                It.IsAny<List<WatchDogLogEventWithCountAndDate>>(),
                It.IsAny<List<WatchDogLogEventWithCountAndDate>>(),
                It.IsAny<List<WatchDogLogEventWithCountAndDate>>(),
                It.IsAny<List<Location>>(),
                It.IsAny<List<ApplicationUser>>(),
                It.IsAny<List<Jurisdiction>>(),
                It.IsAny<List<UserJurisdiction>>(),
                It.IsAny<List<Area>>(),
                It.IsAny<List<UserArea>>(),
                It.IsAny<List<Region>>(),
                It.IsAny<List<UserRegion>>(),
                It.IsAny<List<WatchDogLogEvent>>()), Times.Never);
        }

        [Fact]
        public async void StartScan_WeekdayOnlyFalse_ShouldCreateEmail()
        {
            // Arrange
            var mockDependencies = SetupMocks();
            var emailOptions = new WatchdogEmailOptions
            {
                WeekdayOnly = false,
                ScanDate = new DateTime(2024, 12, 14) // Saturday
            };
            var loggingOptions = new WatchdogLoggingOptions();
            var cancellationToken = new CancellationToken();

            // Act
            await mockDependencies.ScanService.StartScan(loggingOptions, emailOptions, cancellationToken);

            // Assert
            mockDependencies.EmailServiceMock.Verify(service => service.SendAllEmails(
                It.IsAny<WatchdogEmailOptions>(),
                It.IsAny<List<WatchDogLogEventWithCountAndDate>>(),
                It.IsAny<List<WatchDogLogEventWithCountAndDate>>(),
                It.IsAny<List<WatchDogLogEventWithCountAndDate>>(),
                It.IsAny<List<Location>>(),
                It.IsAny<List<ApplicationUser>>(),
                It.IsAny<List<Jurisdiction>>(),
                It.IsAny<List<UserJurisdiction>>(),
                It.IsAny<List<Area>>(),
                It.IsAny<List<UserArea>>(),
                It.IsAny<List<Region>>(),
                It.IsAny<List<UserRegion>>(),
                It.IsAny<List<WatchDogLogEvent>>()), Times.Once);
        }

        private (ScanService ScanService, Mock<IWatchdogEmailService> EmailServiceMock) SetupMocks()
        {
            // Mock dependencies
            var smtpLoggerMock = new Mock<ILogger<SmtpEmailService>>();
            var configurationMock = new Mock<IOptionsSnapshot<EmailConfiguration>>();
            var loggerMock = new Mock<ILogger<ScanService>>();

            var locationRepoMock = new Mock<ILocationRepository>();
            var watchDogLogRepoMock = new Mock<IWatchDogEventLogRepository>();
            var regionsRepoMock = new Mock<IRegionsRepository>();
            var jurisdictionRepoMock = new Mock<IJurisdictionRepository>();
            var areaRepoMock = new Mock<IAreaRepository>();
            var userRegionRepoMock = new Mock<IUserRegionRepository>();
            var userJurisdictionRepoMock = new Mock<IUserJurisdictionRepository>();
            var userAreaRepoMock = new Mock<IUserAreaRepository>();

            var userManagerMock = SetupUserManagerMock();
            var roleManager = SetupRoleManager();

            var emailServiceMock = new Mock<IWatchdogEmailService>();
            SetupRepositories(locationRepoMock, watchDogLogRepoMock);

            var logService = SetupLogService();
            var segmentedErrorsServiceMock = new Mock<SegmentedErrorsService>(watchDogLogRepoMock.Object);
            var ignoreEventServiceMock = new Mock<IWatchDogIgnoreEventService>();

            ignoreEventServiceMock
                .Setup(service => service.GetFilteredWatchDogEventsForEmail(It.IsAny<List<WatchDogLogEvent>>(), It.IsAny<DateTime>()))
                .Returns(
                    new List<WatchDogLogEvent> {
                        new WatchDogLogEvent(1, "1001", new DateTime(2023, 7, 24), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.RecordCount, "Details 1", null),
                    }
                );


            // Initialize ScanService
            var scanService = new ScanService(
                locationRepoMock.Object,
                watchDogLogRepoMock.Object,
                regionsRepoMock.Object,
                jurisdictionRepoMock.Object,
                areaRepoMock.Object,
                userRegionRepoMock.Object,
                userJurisdictionRepoMock.Object,
                userAreaRepoMock.Object,
                userManagerMock.Object,
                roleManager,
                logService,
                emailServiceMock.Object,
                loggerMock.Object,
                segmentedErrorsServiceMock.Object,
                ignoreEventServiceMock.Object);

            return (scanService, emailServiceMock);
        }

        private Mock<UserManager<ApplicationUser>> SetupUserManagerMock()
        {
            return new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
        }

        private RoleManager<IdentityRole> SetupRoleManager()
        {
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleValidators = new List<IRoleValidator<IdentityRole>> { new RoleValidator<IdentityRole>() };
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var identityErrorDescriber = new IdentityErrorDescriber();
            var roleLoggerMock = new Mock<ILogger<RoleManager<IdentityRole>>>();

            return new RoleManager<IdentityRole>(
                roleStoreMock.Object,
                roleValidators,
                keyNormalizerMock.Object,
                identityErrorDescriber,
                roleLoggerMock.Object);
        }

        private void SetupRepositories(Mock<ILocationRepository> locationRepoMock, Mock<IWatchDogEventLogRepository> watchDogLogRepoMock)
        {
            var timestamp1 = DateTime.UtcNow.AddDays(-3);
            var timestamp2 = timestamp1.AddDays(1);
            var timestamp3 = timestamp2.AddDays(1);

            var dayBeforeScanDate = new List<WatchDogLogEvent>{
                new WatchDogLogEvent(1, "1001", new DateTime(2023, 7, 5), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.RecordCount, "Details 1", null),
            };
            var last12MonthsLogs = new List<WatchDogLogEvent> {
                new WatchDogLogEvent(1, "1001", new DateTime(2023, 7, 24), WatchDogComponentTypes.Location, 1, WatchDogIssueTypes.RecordCount, "Details 1", null),
            };

            locationRepoMock
                .Setup(repo => repo.GetLatestVersionOfAllLocations(It.IsAny<DateTime>()))
                .Returns(new List<Location>()
                {
                    new Location()
                    {
                        LocationIdentifier = "location1",
                        Id = 1,
                    }
                });

            watchDogLogRepoMock
                .Setup(repo => repo.GetList())
                .Returns(new List<WatchDogLogEvent>() {
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
                }.AsQueryable());

            watchDogLogRepoMock
                .SetupSequence(repo => repo.GetList(It.IsAny<Expression<Func<WatchDogLogEvent, bool>>>()))
                .Returns(dayBeforeScanDate)
                .Returns(last12MonthsLogs)
                .Returns(last12MonthsLogs);
        }

        private WatchDogLogService SetupLogService()
        {
            var controllerEventLogRepositoryMock = new Mock<IIndianaEventLogRepository>();
            var phaseServiceMock = new Mock<PhaseService>();
            var planService = new PlanService();
            var analyisPhaseService = new AnalysisPhaseService(new PhaseService());
            var analysisPhaseCollectionService = new AnalysisPhaseCollectionService(planService, analyisPhaseService);
            var watchdogLoggerMock = new Mock<ILogger<WatchDogLogService>>();

            phaseServiceMock
                .Setup(service => service.GetPhases(It.IsAny<Location>()))
                .Returns(new List<PhaseDetail>()
                {
                    new PhaseDetail(){ PhaseNumber = 1, UseOverlap = false},
                    new PhaseDetail(){ PhaseNumber = 2, UseOverlap = false},
                });

            controllerEventLogRepositoryMock
                .Setup(repo => repo.GetEventsBetweenDates(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Returns(new List<IndianaEvent> {
            new IndianaEvent { EventCode = 1, EventParam = 111, LocationIdentifier = "7115", Timestamp = new DateTime() }
                });

            return new WatchDogLogService(
                controllerEventLogRepositoryMock.Object,
                analysisPhaseCollectionService,
                phaseServiceMock.Object,
                watchdogLoggerMock.Object);
        }
    }
}
