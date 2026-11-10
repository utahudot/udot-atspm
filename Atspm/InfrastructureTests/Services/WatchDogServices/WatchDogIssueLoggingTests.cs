using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Concurrent;
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
    public class WatchDogIssueLoggingTests
    {
        private static readonly DateTime ScanDate = new DateTime(2026, 11, 3);

        [Theory]
        [InlineData(WatchDogIssueTypes.UnconfiguredApproach, "EventParam/phase 2")]
        [InlineData(WatchDogIssueTypes.UnconfiguredDetector, "EventParam/channel 2")]
        [InlineData(WatchDogIssueTypes.LowDetectorHits, "Detector hit count 0 < LowHitThreshold 3")]
        [InlineData(WatchDogIssueTypes.MissingMainlineData, "Event count 0 == 0")]
        [InlineData(WatchDogIssueTypes.StuckQueueDetection, "Event count 1 > 0")]
        public void Pm_LogsOnlyAddedIssues_WithEvidence(WatchDogIssueTypes issueType, string expectedReason)
        {
            var logger = new RecordingLogger<WatchDogPmLogService>();
            var service = new WatchDogPmLogService(Mock.Of<IIndianaEventLogRepository>(), new PhaseService(), logger);
            var location = Location();
            if (issueType == WatchDogIssueTypes.LowDetectorHits)
            {
                var approach = new Approach { DirectionType = new DirectionType { Description = "Northbound" } };
                approach.Detectors.Add(new Detector
                {
                    Id = 12, DetectorChannel = 2, Approach = approach,
                    DetectionTypes = new List<DetectionType>
                    {
                        new DetectionType { Id = DetectionTypes.AC,
                            MeasureTypes = new List<MeasureType> { new MeasureType { Id = 6 } } }
                    }
                });
                location.Approaches.Add(approach);
            }
            var options = new WatchdogPmLoggingOptions
            {
                PmScanDate = ScanDate, PmPeakStartHour = 17, PmPeakEndHour = 18,
                RampMainlineStartHour = 15, RampMainlineEndHour = 19,
                RampStuckQueueStartHour = 1, RampStuckQueueEndHour = 4, LowHitThreshold = 3
            };
            var errors = new ConcurrentBag<WatchDogLogEvent>();
            void Check()
            {
                switch (issueType)
                {
                    case WatchDogIssueTypes.LowDetectorHits:
                        service.CheckForLowDetectorHits(location, options, new List<IndianaEvent>(), errors, new List<short> { 81, 82 });
                        break;
                    case WatchDogIssueTypes.UnconfiguredApproach:
                        service.CheckForUnconfiguredApproaches(location, options, errors, new List<IndianaEvent> { Event(17, 1, 2) });
                        break;
                    case WatchDogIssueTypes.UnconfiguredDetector:
                        service.CheckForUnconfiguredDetectors(location, options, new List<IndianaEvent> { Event(17, 82, 2) }, errors, new List<short> { 81, 82 });
                        break;
                    case WatchDogIssueTypes.MissingMainlineData:
                        service.CheckMainlineDetections(location, options, new List<IndianaEvent>(), errors);
                        break;
                    case WatchDogIssueTypes.StuckQueueDetection:
                        service.CheckStuckQueueDetections(location, options, new List<IndianaEvent> { Event(2, 1171, 2) }, errors);
                        break;
                }
            }

            Check();
            Check(); // Duplicate candidates must not produce another "added" log.

            AssertIssueLog(logger, Assert.Single(errors), "PM", expectedReason);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, false)]
        public async Task Pm_LogsRecordThresholdOnlyWhenAdded(int count, bool expectedIssue)
        {
            var logger = new RecordingLogger<WatchDogPmLogService>();
            var repository = new Mock<IIndianaEventLogRepository>();
            repository.Setup(r => r.GetEventsBetweenDates("LOC1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Enumerable.Range(0, count).Select(_ => Event(17, 99, 1)).ToList());
            var service = new WatchDogPmLogService(repository.Object, new PhaseService(), logger);
            var options = new WatchdogPmLoggingOptions
            {
                PmScanDate = ScanDate, PmPeakStartHour = 17, PmPeakEndHour = 18, MinimumRecords = 0
            };

            var errors = await service.GetWatchDogIssues(options, new List<Location> { Location() }, CancellationToken.None);

            if (expectedIssue)
                AssertIssueLog(logger, Assert.Single(errors), "PM", "Unique record count 0 <= MinimumRecords 0");
            else
            {
                Assert.Empty(errors);
                Assert.Empty(logger.Issues);
            }
        }

        [Theory]
        [InlineData(WatchDogIssueTypes.StuckPed, "Pedestrian event count 2 > MaximumPedestrianEvents 1")]
        [InlineData(WatchDogIssueTypes.ForceOffThreshold, "Force-off fraction 1 > PercentThreshold 0.9")]
        [InlineData(WatchDogIssueTypes.MaxOutThreshold, "Max-out fraction 1 > PercentThreshold 0.9")]
        public async Task Am_LogsOnlyAddedIssues_WithThresholds(WatchDogIssueTypes issueType, string expectedReason)
        {
            var logger = new RecordingLogger<WatchDogAmLogService>();
            var service = new WatchDogAmLogService(Mock.Of<IIndianaEventLogRepository>(), null, logger);
            var location = Location();
            var approach = new Approach { Id = 12, Location = location };
            var phase = new AnalysisPhaseData
            {
                PhaseNumber = 2, PercentForceOffs = 1, PercentMaxOuts = 1, TotalPhaseTerminations = 2,
                PedestrianEvents = new List<IndianaEvent> { Event(2, 21, 2), Event(2, 23, 2) },
                TerminationEvents = new List<IndianaEvent> { Event(2, 6, 2), Event(3, 6, 2) }
            };
            var options = new WatchdogAmLoggingOptions
            {
                AmScanDate = ScanDate, AmStartHour = 1, AmEndHour = 5,
                MaximumPedestrianEvents = 1, PercentThreshold = .9, MinPhaseTerminations = 1
            };
            var errors = new ConcurrentBag<WatchDogLogEvent>();
            async Task Check()
            {
                if (issueType == WatchDogIssueTypes.StuckPed)
                    await service.CheckForStuckPed(phase, approach, options, errors);
                else if (issueType == WatchDogIssueTypes.ForceOffThreshold)
                    await service.CheckForForceOff(phase, approach, options, errors);
                else
                    await service.CheckForMaxOut(phase, approach, options, errors);
            }

            await Check();
            await Check();

            AssertIssueLog(logger, Assert.Single(errors), "AM", expectedReason);
        }

        [Theory]
        [InlineData(false, false, "Detector hit count 0 < LowHitRampThreshold 3")]
        [InlineData(true, false, "No events with codes")]
        [InlineData(true, true, "Missed intervals 179 > RampMissedEventsThreshold 3")]
        public void Ramp_LogsOnlyAddedIssues_WithEvidence(bool missedEvents, bool hasMainlineEvent, string expectedReason)
        {
            var logger = new RecordingLogger<WatchDogRampLogService>();
            var service = new WatchDogRampLogService(Mock.Of<IIndianaEventLogRepository>(), logger);
            var location = Location();
            location.LocationTypeId = 2;
            location.Approaches.Add(new Approach
            {
                Detectors = new List<Detector>
                {
                    new Detector { Id = 12, DetectorChannel = 2,
                        DetectionTypes = new List<DetectionType> { new DetectionType { Id = DetectionTypes.P } } }
                }
            });
            var options = new WatchdogRampLoggingOptions
            {
                RampMissedDetectorHitsStartScanDate = ScanDate, RampMissedDetectorHitsEndScanDate = ScanDate,
                RampMissedDetectorHitStartHour = 7, RampMissedDetectorHitEndHour = 8,
                RampDetectorStartHour = 7, RampDetectorEndHour = 8,
                LowHitRampThreshold = 3, RampMissedEventsThreshold = 3
            };
            var events = hasMainlineEvent ? new List<IndianaEvent> { Event(7, 1371, 1700) } : new List<IndianaEvent>();
            var errors = new ConcurrentBag<WatchDogLogEvent>();
            void Check()
            {
                if (missedEvents)
                    service.CheckRampMissedDetectorHits(location, options, events, errors);
                else
                    service.CheckForLowRampDetectorHits(location, options, events, errors, new List<short> { 81, 82 });
            }

            Check();
            Check();

            AssertIssueLog(logger, Assert.Single(errors), "Ramp", expectedReason);
        }

        private static void AssertIssueLog<T>(RecordingLogger<T> logger, WatchDogLogEvent issue, string scanType, string expectedReason)
        {
            var fields = Assert.Single(logger.Issues);
            Assert.Equal(scanType, fields["scanType"]);
            Assert.Equal(issue.IssueType, fields["issueType"]);
            Assert.Equal(issue.LocationIdentifier, fields["locationIdentifier"]);
            Assert.Equal(issue.LocationId, fields["locationId"]);
            Assert.Equal(issue.Timestamp, fields["scanDate"]);
            Assert.Equal(issue.ComponentId, fields["componentId"]);
            Assert.Equal(issue.ComponentType, fields["componentType"]);
            Assert.Equal(issue.Phase, fields["phase"]);
            Assert.Equal(issue.Key, fields["key"]);
            Assert.Equal(issue.Details, fields["details"]);
            Assert.Contains(expectedReason, (string)fields["reason"]);
            Assert.Contains(ScanDate.ToString("yyyy-MM-dd"), (string)fields["reason"]);
        }

        private static Location Location() => new Location
        {
            Id = 1, LocationIdentifier = "LOC1", Approaches = new List<Approach>(), Devices = new List<Device>()
        };

        private static IndianaEvent Event(int hour, short code, short parameter) => new IndianaEvent
        {
            LocationIdentifier = "LOC1", Timestamp = ScanDate.AddHours(hour), EventCode = code, EventParam = parameter
        };

        private sealed class RecordingLogger<T> : ILogger<T>
        {
            public List<Dictionary<string, object>> Issues { get; } = new List<Dictionary<string, object>>();
            public bool IsEnabled(LogLevel logLevel) => true;
            public IDisposable BeginScope<TState>(TState state) => null;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (eventId.Name != "Watchdog Issue Added")
                    return;
                Assert.Equal(LogLevel.Information, logLevel);
                Assert.Equal(1253, eventId.Id);
                Issues.Add(((IEnumerable<KeyValuePair<string, object>>)state).ToDictionary(p => p.Key, p => p.Value));
            }
        }
    }
}
