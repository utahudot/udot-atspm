using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Xunit;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices.Tests
{
    public class WatchDogPmLogServiceTests
    {
        private readonly Mock<IIndianaEventLogRepository> _controllerEventLogRepositoryMock;
        private readonly Mock<PhaseService> _phaseServiceMock;
        private readonly Mock<ILogger<WatchDogRampLogService>> _loggerMock;
        private readonly WatchDogPmLogService _watchDogPmLogService;

        public WatchDogPmLogServiceTests()
        {
            _controllerEventLogRepositoryMock = new Mock<IIndianaEventLogRepository>();
            _phaseServiceMock = new Mock<PhaseService>();
            _loggerMock = new Mock<ILogger<WatchDogRampLogService>>();

            _watchDogPmLogService = new WatchDogPmLogService(
                _controllerEventLogRepositoryMock.Object,
                _phaseServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public void Constructor_Should_Create_Instance()
        {
            Assert.NotNull(_watchDogPmLogService);
        }

        [Fact]
        public async Task CheckLocationRecordCount_Should_ReturnError_WhenRecordsAreInsufficient()
        {
            var location = new Location
            {
                Id = 1,
                LocationIdentifier = "LOC1",
                Devices = new List<Device> { new Device { Ipaddress = "127.0.0.1" } }
            };

            var options = new WatchdogPmLoggingOptions
            {
                PmScanDate = DateTime.Today,
                MinimumRecords = 5
            };

            var locationEvents = new List<IndianaEvent> { new IndianaEvent() }; // only 1 < MinimumRecords

            var errors = await _watchDogPmLogService.CheckLocationRecordCount(options.PmScanDate, location, options, locationEvents);

            Assert.NotNull(errors);
            Assert.Equal(WatchDogIssueTypes.RecordCount, errors.IssueType);
        }

        [Fact]
        public async Task CheckLocationRecordCount_Should_ReturnNull_WhenRecordsAreSufficient()
        {
            var location = new Location
            {
                Id = 1,
                LocationIdentifier = "LOC1",
                Devices = new List<Device> { new Device { Ipaddress = "127.0.0.1" } }
            };

            var options = new WatchdogPmLoggingOptions
            {
                PmScanDate = DateTime.Today,
                MinimumRecords = 1
            };

            var locationEvents = new List<IndianaEvent> { new IndianaEvent(), new IndianaEvent() };

            var errors = await _watchDogPmLogService.CheckLocationRecordCount(options.PmScanDate, location, options, locationEvents);

            Assert.Null(errors);
        }

        [Fact]
        public void CheckForUnconfiguredApproaches_Should_AddError_WhenPhaseNotConfigured()
        {
            var location = new Location
            {
                Id = 1,
                LocationIdentifier = "LOC1"
            };

            // Mock PhaseService to return empty list
            _phaseServiceMock.Setup(p => p.GetPhases(location)).Returns(new List<PhaseDetail>());

            var options = new WatchdogPmLoggingOptions
            {
                PmScanDate = DateTime.Today,
                PmPeakStartHour = 6,
                PmPeakEndHour = 9,
                RampMainlineStartHour = 6,
                RampMainlineEndHour = 9,
                RampStuckQueueStartHour = 6,
                RampStuckQueueEndHour = 9,
                MinimumRecords = 5,
                LowHitThreshold = 0,
                //RampDetectorStartHour = 6,
                //RampDetectorEndHour = 9,
                //LowHitRampThreshold = 0
            };

            var errors = new ConcurrentBag<WatchDogLogEvent>();

            var cycles = new List<IndianaEvent>
            {
                new IndianaEvent { EventCode = 1, EventParam = 1, Timestamp = DateTime.Today.AddHours(7) }
            };

            _watchDogPmLogService.CheckForUnconfiguredApproaches(location, options, errors, cycles);

            Assert.Single(errors);
            Assert.Equal(WatchDogIssueTypes.UnconfiguredApproach, errors.First().IssueType);
        }

        [Fact]
        public void CheckForUnconfiguredDetectors_Should_AddError_WhenDetectorNotConfigured()
        {
            var location = new Location
            {
                Id = 1,
                LocationIdentifier = "LOC1",
                // No detectors configured
                Devices = new List<Device>
                {
                    new Device { Ipaddress = "192.168.1.1" },
                    new Device { Ipaddress = "192.168.1.2" }
                }
            };

            var options = new WatchdogPmLoggingOptions
            {
                PmScanDate = DateTime.Today,
                PmPeakStartHour = 6,
                PmPeakEndHour = 9,
                RampMainlineStartHour = 6,
                RampMainlineEndHour = 9,
                RampStuckQueueStartHour = 6,
                RampStuckQueueEndHour = 9,
                MinimumRecords = 5,
                LowHitThreshold = 0,
                //RampDetectorStartHour = 6,
                //RampDetectorEndHour = 9,
                //LowHitRampThreshold = 0
            };

            var errors = new ConcurrentBag<WatchDogLogEvent>();

            var locationEvents = new List<IndianaEvent>
            {
                new IndianaEvent { EventCode = 81, EventParam = 1, Timestamp = DateTime.Today.AddHours(7) }
            };

            WatchDogPmLogService.CheckForUnconfiguredDetectors(location, options, locationEvents, errors, new List<short> { 81 });

            Assert.Single(errors);
            Assert.Equal(WatchDogIssueTypes.UnconfiguredDetector, errors.First().IssueType);
        }

        [Fact]
        public void CheckMainlineDetections_Should_AddError_WhenEventsMissing()
        {
            var location = new Location
            {
                Id = 1,
                LocationIdentifier = "LOC1"
            };

            var options = new WatchdogPmLoggingOptions
            {
                PmScanDate = DateTime.Today,
                RampMainlineStartHour = 6,
                RampMainlineEndHour = 9
            };

            var errors = new ConcurrentBag<WatchDogLogEvent>();
            var locationEvents = new List<IndianaEvent>(); // empty => missing mainline data

            _watchDogPmLogService.CheckMainlineDetections(location, options, locationEvents, errors);

            Assert.Single(errors);
            Assert.Equal(WatchDogIssueTypes.MissingMainlineData, errors.First().IssueType);
        }

        [Fact]
        public void CheckStuckQueueDetections_Should_AddError_WhenEventsExist()
        {
            var location = new Location
            {
                Id = 1,
                LocationIdentifier = "LOC1"
            };

            var options = new WatchdogPmLoggingOptions
            {
                PmScanDate = DateTime.Today,
                RampStuckQueueStartHour = 6,
                RampStuckQueueEndHour = 9
            };

            var errors = new ConcurrentBag<WatchDogLogEvent>();
            var locationEvents = new List<IndianaEvent>
            {
                new IndianaEvent { EventCode = 1171, Timestamp = DateTime.Today.AddHours(7) }
            };

            _watchDogPmLogService.CheckStuckQueueDetections(location, options, locationEvents, errors);

            Assert.Single(errors);
            Assert.Equal(WatchDogIssueTypes.StuckQueueDetection, errors.First().IssueType);
        }

    }
}
