using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public class WatchDogEventDeduplicationTests
    {
        private static readonly DateTime ScanDate = new DateTime(2026, 11, 2);

        [Theory]
        [InlineData(3, true)]
        [InlineData(2, false)]
        public async Task Pm_CountsUniqueEventsAcrossOverlappingWindows(int minimumRecords, bool missingRecords)
        {
            var location = CreateLocation();
            // Same time, different codes and parameters are three distinct records.
            var events = new[] { Event(17, 81, 1), Event(17, 82, 1), Event(17, 81, 2) };
            var repository = CreateRepository(events);
            var service = new WatchDogPmLogService(repository.Object, new PhaseService(), NullLogger<WatchDogPmLogService>.Instance);
            var options = new WatchdogPmLoggingOptions
            {
                PmScanDate = ScanDate,
                PmPeakStartHour = 17,
                PmPeakEndHour = 18,
                RampMainlineStartHour = 15,
                RampMainlineEndHour = 19,
                RampStuckQueueStartHour = 1,
                RampStuckQueueEndHour = 4,
                MinimumRecords = minimumRecords
            };

            var errors = await service.GetWatchDogIssues(options, new List<Location> { location }, CancellationToken.None);

            Assert.Equal(missingRecords, errors.Any(e => e.IssueType == WatchDogIssueTypes.RecordCount));
            repository.Verify(r => r.GetEventsBetweenDates(location.LocationIdentifier, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Exactly(3));
        }

        [Fact]
        public async Task Ramp_CountsUniqueDetectorHitsAcrossOverlappingWindows()
        {
            var location = CreateLocation();
            location.LocationTypeId = 2;
            location.Approaches.Add(new Approach
            {
                Detectors = new List<Detector>
                {
                    new Detector
                    {
                        Id = 1,
                        DetectorChannel = 1,
                        DetectionTypes = new List<DetectionType> { new DetectionType { Id = DetectionTypes.P } }
                    }
                }
            });
            var repository = CreateRepository(new[] { Event(7, 81, 1), Event(7, 82, 1) });
            var service = new WatchDogRampLogService(repository.Object, NullLogger<WatchDogRampLogService>.Instance);
            var options = new WatchdogRampLoggingOptions
            {
                RampMissedDetectorHitsStartScanDate = ScanDate,
                RampMissedDetectorHitsEndScanDate = ScanDate,
                RampMissedDetectorHitStartHour = 6,
                RampMissedDetectorHitEndHour = 9,
                RampDetectorStartHour = 7,
                RampDetectorEndHour = 8,
                LowHitRampThreshold = 3
            };

            var errors = await service.GetWatchDogIssues(options, new List<Location> { location }, CancellationToken.None);

            var lowHits = Assert.Single(errors, e => e.IssueType == WatchDogIssueTypes.LowRampDetectorHits);
            Assert.Equal("CH: 1 - Count: 2", lowHits.Details);
        }

        [Fact]
        public async Task Am_CountsUniquePedestrianEventsFromDuplicateSourceRecords()
        {
            var location = CreateLocation();
            location.Approaches.Add(new Approach { Id = 1, ProtectedPhaseNumber = 1, Location = location });
            var events = new[]
            {
                Event(2, 1, 1), Event(2, 21, 1), Event(2, 23, 1),
                Event(2, 8, 1), Event(2, 11, 1)
            };
            var repository = CreateRepository(events.Concat(events).ToArray());
            var phases = new Mock<PhaseService>();
            phases.Setup(p => p.GetPhases(location)).Returns(new List<PhaseDetail>());
            var analysis = new AnalysisPhaseCollectionService(new PlanService(), new AnalysisPhaseService(phases.Object));
            var service = new WatchDogAmLogService(repository.Object, analysis, NullLogger<WatchDogAmLogService>.Instance);
            var options = new WatchdogAmLoggingOptions
            {
                AmScanDate = ScanDate,
                AmStartHour = 1,
                AmEndHour = 5,
                MaximumPedestrianEvents = 1,
                ConsecutiveCount = 3,
                MinPhaseTerminations = 50,
                PercentThreshold = .9
            };

            var errors = await service.GetWatchDogIssues(options, new List<Location> { location }, CancellationToken.None);

            var stuckPed = Assert.Single(errors, e => e.IssueType == WatchDogIssueTypes.StuckPed);
            Assert.Equal("2 Pedestrian Activations", stuckPed.Details);
        }

        private static Location CreateLocation() => new Location
        {
            Id = 1,
            LocationIdentifier = "LOC1",
            Approaches = new List<Approach>(),
            Devices = new List<Device>()
        };

        private static IndianaEvent Event(int hour, short code, short parameter) => new IndianaEvent
        {
            LocationIdentifier = "LOC1",
            Timestamp = ScanDate.AddHours(hour),
            EventCode = code,
            EventParam = parameter
        };

        private static Mock<IIndianaEventLogRepository> CreateRepository(IReadOnlyList<IndianaEvent> events)
        {
            var repository = new Mock<IIndianaEventLogRepository>();
            repository.Setup(r => r.GetEventsBetweenDates("LOC1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((string location, DateTime start, DateTime end) => events
                    .Where(e => e.Timestamp >= start && e.Timestamp < end)
                    // Each query returns separate objects, as the real repository does.
                    .Select(e => new IndianaEvent
                    {
                        LocationIdentifier = e.LocationIdentifier,
                        Timestamp = e.Timestamp,
                        EventCode = e.EventCode,
                        EventParam = e.EventParam
                    }).ToList());
            return repository;
        }
    }
}
