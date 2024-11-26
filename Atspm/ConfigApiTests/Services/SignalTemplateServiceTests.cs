using Moq;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Xunit;
using Assert = Xunit.Assert;

namespace Utah.Udot.Atspm.ConfigApi.Services.Tests
{
    public class SignalTemplateServiceTests
    {
        private Mock<IApproachRepository> _approachRepositoryMock;
        private Mock<ILocationRepository> _locationRepositoryMock;
        private Mock<IDetectionTypeRepository> _detectionTypeRepositoryMock;
        private Mock<IEventLogRepository> _eventLogRepositoryMock;
        private SignalTemplateService _service;

        public SignalTemplateServiceTests()
        {
            _approachRepositoryMock = new Mock<IApproachRepository>();
            _locationRepositoryMock = new Mock<ILocationRepository>();
            _detectionTypeRepositoryMock = new Mock<IDetectionTypeRepository>();
            _eventLogRepositoryMock = new Mock<IEventLogRepository>();

            _service = new SignalTemplateService(
                _locationRepositoryMock.Object,
                _approachRepositoryMock.Object,
                _detectionTypeRepositoryMock.Object,
                _eventLogRepositoryMock.Object
            );
        }

        [Fact]
        public void ModifyLocationWithEvents_ProcessesEventsCorrectly()
        {
            DateTime specificDateTime = new DateTime(2024, 10, 15, 15, 30, 0);
            // Arrange
            var mockLocation = new Location
            {
                Id = 1,
                Approaches = new List<Approach>
                {
                    new Approach {
                        PermissivePhaseNumber = 10,
                        Detectors = new List<Detector> {
                        new Detector { DetectorChannel = 1 },
                        new Detector { DetectorChannel = 2 }
                    }},
                    new Approach {
                        PermissivePhaseNumber = 16,
                        Detectors = new List<Detector> {
                        new Detector { DetectorChannel = 1 }
                    }},
                    new Approach {
                        PedestrianPhaseNumber = 9,
                        Detectors = new List<Detector> {
                        new Detector { DetectorChannel = 10 },
                        new Detector { DetectorChannel = 8 }
                    }},
                    new Approach {
                        PermissivePhaseNumber = 5,
                        IsPermissivePhaseOverlap = true,
                        Detectors = new List<Detector> {
                        new Detector { DetectorChannel = 1 }
                    }},
                    new Approach {
                        ProtectedPhaseNumber = 1,
                        Detectors = new List<Detector> {
                        new Detector { DetectorChannel = 5 },
                        new Detector { DetectorChannel = 7 },
                        new Detector { DetectorChannel = 18 }
                    } }
                }
            };

            var mockEvents = new List<IndianaEvent>
            {
                new IndianaEvent {
                    EventParam = 1,
                    EventCode = 82,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 2,
                    EventCode = 82,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime.AddMinutes(5)
                },
                new IndianaEvent {
                    EventParam = 3,
                    EventCode = 82,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 4,
                    EventCode = 82,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime.AddMinutes(5)
                },
                new IndianaEvent {
                    EventParam = 5,
                    EventCode = 82,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 6,
                    EventCode = 82,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime.AddMinutes(5)
                },
                new IndianaEvent {
                    EventParam = 7,
                    EventCode = 82,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 8,
                    EventCode = 82,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime.AddMinutes(5)
                },
                new IndianaEvent {
                    EventParam = 1,
                    EventCode = 1,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 10,
                    EventCode = 8,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 3,
                    EventCode = 11,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 5,
                    EventCode = 61,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 12,
                    EventCode = 63,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 7,
                    EventCode = 64,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 8,
                    EventCode = 90,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                },
                new IndianaEvent {
                    EventParam = 9,
                    EventCode = 90,
                    LocationIdentifier = "location1",
                    Timestamp = specificDateTime
                }
            };

            // Act
            var result = SignalTemplateService.ModifyLocationWithEvents(mockLocation, mockEvents);
            var LoggedButUnUsedChannels = new List<short> { 3, 4, 6 };
            var LoggedButUnUsedOverlap = new List<short> { 12, 7 };
            var LoggedButUnUsedPedestrian = new List<short> { 8 };
            var LoggedButUnUsedPermissiveOrProtected = new List<short> { 3 };
            var RemovedApproachPermissivePhase = 16;
            var RemovedChannel = 18;

            // Assert
            Assert.True(result.Location.Approaches.Any());
            Assert.True(result.Location.Approaches.Any(a => a.PermissivePhaseNumber == 10));
            Assert.False(result.Location.Approaches.Any(a => a.PermissivePhaseNumber == RemovedApproachPermissivePhase));
            Assert.False(result.Location.Approaches.Any(a => a.Detectors.Any(d => d.DetectorChannel == RemovedChannel)));
            Assert.Equal(4, result.Location.Approaches.Count);

            Assert.True(result.RemovedApproaches.Any());
            Assert.True(result.RemovedApproaches.Any(a => a.PermissivePhaseNumber == RemovedApproachPermissivePhase));
            Assert.Equal(1, result.RemovedApproaches.Count);

            Assert.True(result.LoggedButUnusedDetectorChannels.Any());
            Assert.Equal(LoggedButUnUsedChannels, result.LoggedButUnusedDetectorChannels);
            Assert.Equal(LoggedButUnUsedChannels.Count, result.LoggedButUnusedDetectorChannels.Count);

            Assert.True(result.LoggedButUnusedOverlapPhases.Any());
            Assert.Equal(LoggedButUnUsedOverlap, result.LoggedButUnusedOverlapPhases);
            Assert.Equal(LoggedButUnUsedOverlap.Count, result.LoggedButUnusedOverlapPhases.Count);

            Assert.True(result.LoggedButUnusedPedestrianPhases.Any());
            Assert.Equal(LoggedButUnUsedPedestrian, result.LoggedButUnusedPedestrianPhases);
            Assert.Equal(LoggedButUnUsedPedestrian.Count, result.LoggedButUnusedPedestrianPhases.Count);

            Assert.True(result.LoggedButUnusedProtectedOrPermissivePhases.Any());
            Assert.Equal(LoggedButUnUsedPermissiveOrProtected, result.LoggedButUnusedProtectedOrPermissivePhases);
            Assert.Equal(LoggedButUnUsedPermissiveOrProtected.Count, result.LoggedButUnusedProtectedOrPermissivePhases.Count);

            Location newLocation = result.Location;

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
