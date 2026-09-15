using Moq;
using Utah.Udot.Atspm.Business.TimeOfDay;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Utah.Udot.Atspm.Repositories.AggregationRepositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.ReportApi.ReportServices;

namespace ReportApiTests
{
    public class TimeOfDayReportServiceTests
    {
        [Fact]
        public void NormalizeForExecution_KeepsOmittedBinSizeAtFixedFifteenMinutes()
        {
            var options = new TimeOfDayOptions();

            TimeOfDayReportService.NormalizeForExecution(options);

            Assert.Equal(15, options.BinSizeMinutes);
        }

        [Theory]
        [InlineData(15)]
        [InlineData(30)]
        public void NormalizeForExecution_ForcesFixedFifteenMinuteBinSize(int requestedBinSizeMinutes)
        {
            var options = new TimeOfDayOptions
            {
                BinSizeMinutes = requestedBinSizeMinutes
            };

            TimeOfDayReportService.NormalizeForExecution(options);

            Assert.Equal(15, options.BinSizeMinutes);
        }

        [Fact]
        public async Task ExecuteAsync_MergesConsecutiveIndianaWindowsAndCountsOverlapOnce()
        {
            var firstDate = new DateOnly(2026, 3, 18);
            var secondDate = firstDate.AddDays(1);
            var firstStart = firstDate.ToDateTime(TimeOnly.MinValue);
            var events = new List<IndianaEvent>
            {
                IndianaEvent(firstStart.AddDays(-6), IndianaEnumerations.CoordPatternChange, 7),
                IndianaEvent(firstStart.AddHours(23).AddMinutes(30), IndianaEnumerations.VehicleDetectorOn, 7)
            };
            var eventRepository = EventRepository(events);
            var service = CreateService(eventRepository, LocationWithDetector());

            var result = await service.ExecuteAsync(new TimeOfDayOptions
            {
                LocationIdentifiers = new List<string> { "1001" },
                SelectedDates = new List<DateOnly> { firstDate, secondDate }
            }, CancellationToken.None);

            var location = Assert.Single(result.Locations);
            Assert.Equal(4, location.Profile.Points.Single(point => point.Minutes == 23 * 60 + 30).AverageVolume);
            Assert.Contains(location.CurrentPlanSchedule, plan => plan.PlanNumber == "7");
            eventRepository.Verify(repository => repository.GetEventsBetweenDates(
                "1001",
                firstStart.AddDays(-7),
                secondDate.ToDateTime(TimeOnly.MinValue).AddDays(1).AddHours(1)), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ExcludesIndianaDetectorEventsOnUnselectedDates()
        {
            var firstDate = new DateOnly(2026, 3, 18);
            var lastDate = firstDate.AddDays(2);
            var firstStart = firstDate.ToDateTime(TimeOnly.MinValue);
            var lastStart = lastDate.ToDateTime(TimeOnly.MinValue);
            var events = new List<IndianaEvent>
            {
                IndianaEvent(firstStart.AddHours(8), IndianaEnumerations.VehicleDetectorOn, 7),
                IndianaEvent(firstStart.AddDays(1).AddHours(12), IndianaEnumerations.VehicleDetectorOn, 7),
                IndianaEvent(lastStart.AddHours(8), IndianaEnumerations.VehicleDetectorOn, 7)
            };
            var service = CreateService(EventRepository(events), LocationWithDetector());

            var result = await service.ExecuteAsync(new TimeOfDayOptions
            {
                LocationIdentifiers = new List<string> { "1001" },
                SelectedDates = new List<DateOnly> { firstDate, lastDate }
            }, CancellationToken.None);

            var profile = Assert.Single(result.Locations).Profile;
            Assert.Equal(4, profile.Points.Single(point => point.Minutes == 8 * 60).AverageVolume);
            Assert.Equal(0, profile.Points.Single(point => point.Minutes == 12 * 60).AverageVolume);
        }

        private static TimeOfDayReportService CreateService(
            Mock<IIndianaEventLogRepository> eventRepository,
            Location location)
        {
            var locationRepository = new Mock<ILocationRepository>();
            locationRepository
                .Setup(repository => repository.GetLatestVersionOfLocation(
                    location.LocationIdentifier,
                    It.IsAny<DateTime>()))
                .Returns(location);
            var profileService = new TimeOfDayProfileService();
            var timeOfDayService = new TimeOfDayService(
                new TimeOfDayObservationService(),
                profileService,
                new TimeOfDayRecommendationService(profileService),
                new TimeOfDayPlanScheduleService(),
                new TimeOfDayPlanProfileService(),
                new TimeOfDaySplitPressureService(profileService));

            return new TimeOfDayReportService(
                locationRepository.Object,
                eventRepository.Object,
                new Mock<IDetectorEventCountAggregationRepository>().Object,
                new Mock<ISignalTimingPlanRepository>().Object,
                timeOfDayService);
        }

        private static Mock<IIndianaEventLogRepository> EventRepository(IReadOnlyList<IndianaEvent> events)
        {
            var repository = new Mock<IIndianaEventLogRepository>();
            repository
                .Setup(mock => mock.GetEventsBetweenDates(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Returns((string locationIdentifier, DateTime start, DateTime end) => events
                    .Where(item => item.LocationIdentifier == locationIdentifier && item.Timestamp >= start && item.Timestamp < end)
                    .ToList());
            return repository;
        }

        private static IndianaEvent IndianaEvent(DateTime timestamp, IndianaEnumerations eventCode, short eventParam)
        {
            return new IndianaEvent
            {
                LocationIdentifier = "1001",
                EventCode = (short)eventCode,
                EventParam = eventParam,
                Timestamp = timestamp
            };
        }

        private static Location LocationWithDetector()
        {
            var location = new Location
            {
                LocationIdentifier = "1001",
                PrimaryName = "Main",
                SecondaryName = "State"
            };
            var approach = new Approach
            {
                Location = location,
                DirectionTypeId = DirectionTypes.EB,
                Mph = 35
            };
            approach.Detectors = new List<Detector>
            {
                new()
                {
                    Id = 7,
                    Approach = approach,
                    DetectorChannel = 7,
                    LaneType = LaneTypes.V,
                    MovementType = MovementTypes.T,
                    DetectionTypes = new List<DetectionType>
                    {
                        new()
                        {
                            Id = DetectionTypes.LLC,
                            MeasureTypes = new List<MeasureType> { new() { Id = 5 } }
                        }
                    }
                }
            };
            location.Approaches = new List<Approach> { approach };
            return location;
        }
    }
}
