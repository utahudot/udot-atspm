using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Xunit;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests
{
    public class WatchDogRampLogServiceTests
    {
        private readonly Mock<IIndianaEventLogRepository> _controllerEventLogRepositoryMock;
        private readonly Mock<ILogger<WatchDogRampLogService>> _loggerMock;
        private readonly WatchDogRampLogService _watchDogRampLogService;

        public WatchDogRampLogServiceTests()
        {
            _controllerEventLogRepositoryMock = new Mock<IIndianaEventLogRepository>();
            _loggerMock = new Mock<ILogger<WatchDogRampLogService>>();

            _watchDogRampLogService = new WatchDogRampLogService(
                _controllerEventLogRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public void Constructor_Should_Create_Instance()
        {
            Assert.NotNull(_watchDogRampLogService);
        }

        [Fact]
        public async Task GetWatchDogIssues_Should_ReturnEmptyList_WhenLocationsEmpty()
        {
            // Arrange
            var options = new WatchdogRampLoggingOptions();
            var locations = new List<Location>();

            // Act
            var result = await _watchDogRampLogService.GetWatchDogIssues(options, locations, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void CheckRampMissedDetectorHits_Should_AddError_WhenNoEventsDetected()
        {
            // Arrange
            var options = new WatchdogRampLoggingOptions
            {
                RampMissedDetectorHitsStartScanDate = DateTime.Today.AddHours(6),
                RampMissedDetectorHitsEndScanDate = DateTime.Today.AddHours(9),
                RampMissedEventsThreshold = 0,
                RampDetectorStartHour = 6,
                RampDetectorEndHour = 9,
                LowHitRampThreshold = 0
            };

            var detector = new Detector
            {
                Id = 1,
                DetectorChannel = 1,
                DetectionTypes = new List<DetectionType>
        {
            new DetectionType { Id = (DetectionTypes)8 } // valid type
        }
            };

            var approach = new Approach
            {
                Id = 1,
                LocationId = 1,
                Detectors = new List<Detector> { detector } // detectors belong to approach
            };

            var location = new Location
            {
                Id = 1,
                LocationIdentifier = "LOC1",
                LocationTypeId = 2, // RM type required
                Approaches = new List<Approach> { approach } // approaches belong to location
            };

            var errors = new ConcurrentBag<WatchDogLogEvent>();
            var events = new List<IndianaEvent>(); // empty -> should trigger RampMissedDetectorHits

            // Act
            _watchDogRampLogService.CheckRampMissedDetectorHits(location, options, events, errors);

            // Assert
            Assert.Single(errors);
            var error = errors.First();
            Assert.Equal(WatchDogIssueTypes.RampMissedDetectorHits, error.IssueType);
            Assert.Equal(detector.Id, error.ComponentId);
            //Assert.Contains("No events received", error.Message);
        }

        [Fact]
        public void CheckRampMissedDetectorHits_Should_AddError_WhenMissingData()
        {
            // Arrange
            var options = new WatchdogRampLoggingOptions
            {
                RampMissedDetectorHitsStartScanDate = DateTime.Today.AddHours(6),
                RampMissedDetectorHitsEndScanDate = DateTime.Today.AddHours(9),
                RampMissedEventsThreshold = 0,
                RampDetectorStartHour = 6,
                RampDetectorEndHour = 9,
                LowHitRampThreshold = 0
            };

            var detector = new Detector
            {
                Id = 1,
                DetectorChannel = 1,
                DetectionTypes = new List<DetectionType>
        {
            new DetectionType { Id = (DetectionTypes)8 } // valid type
        }
            };

            var approach = new Approach
            {
                Id = 1,
                LocationId = 1,
                Detectors = new List<Detector> { detector } // detectors belong to approach
            };

            var location = new Location
            {
                Id = 1,
                LocationIdentifier = "LOC1",
                LocationTypeId = 2, // RM type required
                Approaches = new List<Approach> { approach } // approaches belong to location
            };

            var errors = new ConcurrentBag<WatchDogLogEvent>();
            var events = new List<IndianaEvent>
            {
                new IndianaEvent
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = options.RampMissedDetectorHitsStartScanDate.AddMinutes(5),
                    EventCode = 1371,      // example detector on / hit event code
                    EventParam = 1
                },
                new IndianaEvent
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = options.RampMissedDetectorHitsStartScanDate.AddMinutes(10),
                    EventCode = 1372,
                    EventParam = 1
                }
            };

            // Act
            _watchDogRampLogService.CheckRampMissedDetectorHits(location, options, events, errors);

            // Assert
            Assert.Single(errors);
            var error = errors.First();
            Assert.Equal(WatchDogIssueTypes.RampMissedDetectorHits, error.IssueType);
            Assert.Equal(detector.Id, error.ComponentId);
        }

        [Fact]
        public void CheckForLowRampDetectorHits_Should_AddError_WhenHitsBelowThreshold()
        {
            // Arrange
            var options = new WatchdogRampLoggingOptions
            {

                RampMissedDetectorHitsStartScanDate = DateTime.Today.AddHours(6),
                RampMissedDetectorHitsEndScanDate = DateTime.Today.AddHours(9),
                RampMissedEventsThreshold = 0,
                RampDetectorStartHour = 6,
                RampDetectorEndHour = 9,
                LowHitRampThreshold = 5
            };

            var detector = new Detector
            {
                Id = 1,
                DetectorChannel = 1,
                DetectionTypes = new List<DetectionType>
        {
            new DetectionType { Id = (DetectionTypes)8 } // valid detection type
        }
            };

            var approach = new Approach
            {
                Id = 1,
                LocationId = 1,
                Detectors = new List<Detector> { detector }
            };

            var location = new Location
            {
                Id = 1,
                LocationIdentifier = "LOC1",
                LocationTypeId = 2, // required for ramp logic
                Approaches = new List<Approach> { approach }
            };

            var detectorEventCodes = new List<short> { 1371 };

            var locationEvents = new List<IndianaEvent>
            {
                new IndianaEvent
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = options.RampDetectorStart.AddMinutes(10),
                    EventCode = 81,
                    EventParam = 1
                },
                new IndianaEvent
                {
                    LocationIdentifier = location.LocationIdentifier,
                    Timestamp = options.RampDetectorStart.AddMinutes(20),
                    EventCode = 82,
                    EventParam = 1
                }
                // Only 2 hits, below threshold of 5
            };

            var errors = new ConcurrentBag<WatchDogLogEvent>();

            // Act
            _watchDogRampLogService.CheckForLowRampDetectorHits(
                location,
                options,
                locationEvents,
                errors,
                detectorEventCodes);

            // Assert
            Assert.Single(errors);

            var error = errors.First();
            Assert.Equal(WatchDogIssueTypes.LowRampDetectorHits, error.IssueType);
            Assert.Equal(detector.Id, error.ComponentId);
            //Assert.Contains("CH: 1", error.Message);
        }

        [Fact]
        public async Task GetWatchDogIssues_Should_HonorCancellationToken()
        {
            // Arrange
            var options = new WatchdogRampLoggingOptions();
            var location = new Location { Id = 1, LocationIdentifier = "LOC1" };
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await _watchDogRampLogService.GetWatchDogIssues(options, new List<Location> { location }, cts.Token);

            // Assert
            Assert.Empty(result); // Should return immediately due to cancellation
        }


    }
}
