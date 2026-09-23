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
                IndianaEvent(firstStart.AddHours(-12), IndianaEnumerations.CoordPatternChange, 7),
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
            Assert.Empty(location.CurrentPlanSchedule);
            eventRepository.Verify(repository => repository.GetEventsBetweenDates(
                "1001",
                firstStart.AddHours(-12),
                secondDate.ToDateTime(TimeOnly.MinValue).AddDays(1).AddHours(12)), Times.Once);
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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ExecuteAsync_LoadsOnlyTwelveHourPaddingForIsolatedDates(int dateCount)
        {
            var firstDate = new DateOnly(2026, 3, 18);
            var selectedDates = Enumerable.Range(0, dateCount)
                .Select(index => firstDate.AddDays(index * 10))
                .ToList();
            var events = selectedDates.SelectMany(date =>
            {
                var start = date.ToDateTime(TimeOnly.MinValue);
                return new[]
                {
                    IndianaEvent(start.AddDays(-6), IndianaEnumerations.CoordPatternChange, 9),
                    IndianaEvent(start.AddHours(-12), IndianaEnumerations.CoordPatternChange, 7),
                    IndianaEvent(start.AddHours(8), IndianaEnumerations.VehicleDetectorOn, 7),
                    IndianaEvent(start.AddDays(1).AddHours(8), IndianaEnumerations.VehicleDetectorOn, 7)
                };
            }).ToList();
            var eventRepository = EventRepository(events);
            var service = CreateService(eventRepository, LocationWithDetector());

            var result = await service.ExecuteAsync(new TimeOfDayOptions
            {
                LocationIdentifiers = new List<string> { "1001" },
                SelectedDates = selectedDates
            }, CancellationToken.None);

            var location = Assert.Single(result.Locations);
            Assert.Equal(4, location.Profile.Points.Single(point => point.Minutes == 8 * 60).AverageVolume);
            Assert.Empty(location.CurrentPlanSchedule);
            foreach (var date in selectedDates)
            {
                var start = date.ToDateTime(TimeOnly.MinValue);
                eventRepository.Verify(repository => repository.GetEventsBetweenDates(
                    "1001", start.AddHours(-12), start.AddDays(1).AddHours(12)), Times.Once);
            }
            eventRepository.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(TimeOfDayDataSource.IndianaEvents)]
        [InlineData(TimeOfDayDataSource.Aggregated)]
        public async Task ExecuteAsync_AlwaysUsesStoredPlansAndSelectsVolumeDataSource(TimeOfDayDataSource dataSource)
        {
            var selectedDate = new DateOnly(2026, 3, 18);
            var start = selectedDate.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);
            var eventRepository = EventRepository(new[]
            {
                IndianaEvent(start, IndianaEnumerations.CoordPatternChange, 9),
                IndianaEvent(start.AddHours(8), IndianaEnumerations.VehicleDetectorOn, 7)
            });
            var planRepository = new Mock<ISignalTimingPlanRepository>();
            planRepository.Setup(repository => repository.GetList()).Returns(new[]
            {
                new SignalTimingPlan { LocationIdentifier = "1001", PlanNumber = 3, Start = start.AddDays(-30), End = start.AddHours(7) },
                new SignalTimingPlan { LocationIdentifier = "1001", PlanNumber = 7, Start = start.AddHours(7), End = DateTime.MinValue },
                new SignalTimingPlan { LocationIdentifier = "1002", PlanNumber = 11, Start = start, End = end },
                new SignalTimingPlan { LocationIdentifier = "1001", PlanNumber = 12, Start = start.AddDays(-1), End = start },
                new SignalTimingPlan { LocationIdentifier = "1001", PlanNumber = 13, Start = end, End = end.AddDays(1) }
            }.AsQueryable());
            var aggregationRepository = new Mock<IDetectorEventCountAggregationRepository>();
            aggregationRepository.Setup(repository => repository.GetAggregationsBetweenDates("1001", start, end))
                .Returns(new List<DetectorEventCountAggregation>
                {
                    new() { DetectorPrimaryId = 7, Start = start.AddHours(8), EventCount = 2 }
                });
            var service = CreateService(eventRepository, LocationWithDetector(), planRepository, aggregationRepository);

            var result = await service.ExecuteAsync(new TimeOfDayOptions
            {
                LocationIdentifiers = new List<string> { "1001" },
                SelectedDates = new List<DateOnly> { selectedDate },
                DataSource = dataSource
            }, CancellationToken.None);

            var location = Assert.Single(result.Locations);
            Assert.Equal(new[] { "3", "7" }, location.CurrentPlanSchedule.Select(plan => plan.PlanNumber));
            Assert.Equal(start, location.CurrentPlanSchedule[0].Start);
            Assert.Equal(start.AddHours(7), location.CurrentPlanSchedule[0].End);
            Assert.Equal(start.AddHours(7), location.CurrentPlanSchedule[1].Start);
            Assert.Equal(end, location.CurrentPlanSchedule[1].End);
            var dailySchedule = Assert.Single(location.DailyPlanSchedules);
            Assert.Equal(selectedDate, dailySchedule.Date);
            Assert.Equal(new[] { "3", "7" }, dailySchedule.Plans.Select(plan => plan.PlanNumber));
            Assert.DoesNotContain(result.Warnings, warning => warning.Code == "PlanScheduleCarriedForward");
            Assert.Equal(dataSource == TimeOfDayDataSource.Aggregated ? 8 : 4,
                location.Profile.Points.Single(point => point.Minutes == 8 * 60).AverageVolume);
            planRepository.Verify(repository => repository.GetList(), Times.Once);
            eventRepository.Verify(repository => repository.GetEventsBetweenDates(
                "1001", start.AddHours(-12), end.AddHours(12)),
                dataSource == TimeOfDayDataSource.IndianaEvents ? Times.Once() : Times.Never());
            aggregationRepository.Verify(repository => repository.GetAggregationsBetweenDates("1001", start, end),
                dataSource == TimeOfDayDataSource.Aggregated ? Times.Once() : Times.Never());
        }

        [Theory]
        [InlineData(TimeOfDayDataSource.IndianaEvents, 254)]
        [InlineData(TimeOfDayDataSource.Aggregated, 254)]
        [InlineData(TimeOfDayDataSource.IndianaEvents, 100)]
        [InlineData(TimeOfDayDataSource.Aggregated, 100)]
        public async Task ExecuteAsync_IdentifiesPlansCarriedForwardFromAnEarlierDate(
            TimeOfDayDataSource dataSource, short planNumber)
        {
            var date = new DateOnly(2026, 4, 6);
            var lastRecordedStart = new DateTime(2026, 4, 4, 22, 30, 0);
            var plans = new Mock<ISignalTimingPlanRepository>();
            plans.Setup(repository => repository.GetList()).Returns(new[]
            {
                new SignalTimingPlan
                {
                    LocationIdentifier = "1001", PlanNumber = planNumber,
                    Start = lastRecordedStart, End = DateTime.MinValue
                }
            }.AsQueryable());
            var events = EventRepository(Array.Empty<IndianaEvent>());
            var aggregations = new Mock<IDetectorEventCountAggregationRepository>();
            aggregations.Setup(repository => repository.GetAggregationsBetweenDates(
                "1001", date.ToDateTime(TimeOnly.MinValue), date.AddDays(1).ToDateTime(TimeOnly.MinValue)))
                .Returns(new List<DetectorEventCountAggregation>());
            var service = CreateService(events, LocationWithDetector(), plans, aggregations);

            var result = await service.ExecuteAsync(new TimeOfDayOptions
            {
                LocationIdentifiers = new() { "1001" },
                SelectedDates = new() { date },
                DataSource = dataSource
            }, CancellationToken.None);

            // A constant plan is possible; expose the source date without inventing a new plan.
            Assert.Equal(planNumber.ToString(), Assert.Single(Assert.Single(result.Locations).CurrentPlanSchedule).PlanNumber);
            var warning = Assert.Single(result.Warnings.Where(warning => warning.Code == "PlanScheduleCarriedForward"));
            Assert.Equal("1001", warning.LocationIdentifier);
            Assert.Contains("2026-04-06", warning.Message);
            Assert.Contains("2026-04-04 22:30:00", warning.Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ExecuteAsync_RejectsExtremeDatesBeforeLoading(bool maximum)
        {
            var events = EventRepository(Array.Empty<IndianaEvent>());
            var locations = new Mock<ILocationRepository>(MockBehavior.Strict);
            var service = CreateService(events, LocationWithDetector(), locationRepository: locations);
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.ExecuteAsync(new TimeOfDayOptions
            {
                LocationIdentifiers = new() { "1001" },
                SelectedDates = new() { maximum ? DateOnly.MaxValue : DateOnly.MinValue }
            }, CancellationToken.None));
            Assert.Contains("Selected dates must be between", exception.Message);
            locations.VerifyNoOtherCalls();
            events.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(TimeOfDayDataSource.IndianaEvents)]
        [InlineData(TimeOfDayDataSource.Aggregated)]
        public async Task ExecuteAsync_UsesEachDatesDetectorConfiguration(TimeOfDayDataSource dataSource)
        {
            var firstDate = new DateOnly(2026, 3, 18);
            var secondDate = firstDate.AddDays(1);
            var firstStart = firstDate.ToDateTime(TimeOnly.MinValue);
            var secondStart = secondDate.ToDateTime(TimeOnly.MinValue);
            var firstLocation = LocationWithDetector();
            firstLocation.Approaches.Single().Detectors.Single().LaneNumber = 1;
            var secondLocation = LocationWithDetector();
            var secondApproach = secondLocation.Approaches.Single();
            secondApproach.DirectionTypeId = DirectionTypes.NB;
            var secondDetector = secondApproach.Detectors.Single();
            secondDetector.Id = 8;
            secondDetector.DetectorChannel = 8;
            secondDetector.LaneNumber = 1;
            secondApproach.Detectors.Add(new Detector { LaneType = LaneTypes.V, LaneNumber = 2 });
            var locations = new Mock<ILocationRepository>();
            locations.Setup(repository => repository.GetLatestVersionOfLocation("1001", firstStart)).Returns(firstLocation);
            locations.Setup(repository => repository.GetLatestVersionOfLocation("1001", secondStart)).Returns(secondLocation);
            var events = EventRepository(new[]
            {
                IndianaEvent(firstStart.AddHours(8), IndianaEnumerations.VehicleDetectorOn, 7),
                IndianaEvent(firstStart.AddHours(8), IndianaEnumerations.VehicleDetectorOn, 8),
                IndianaEvent(secondStart.AddHours(8), IndianaEnumerations.VehicleDetectorOn, 7),
                IndianaEvent(secondStart.AddHours(8), IndianaEnumerations.VehicleDetectorOn, 8)
            });
            var aggregations = new Mock<IDetectorEventCountAggregationRepository>();
            foreach (var start in new[] { firstStart, secondStart })
            {
                aggregations.Setup(repository => repository.GetAggregationsBetweenDates("1001", start, start.AddDays(1)))
                    .Returns(new List<DetectorEventCountAggregation>
                    {
                        new() { DetectorPrimaryId = 7, Start = start.AddHours(8), EventCount = 1 },
                        new() { DetectorPrimaryId = 8, Start = start.AddHours(8), EventCount = 1 }
                    });
            }
            var result = await CreateService(events, firstLocation, detectorAggregationRepository: aggregations, locationRepository: locations)
                .ExecuteAsync(new TimeOfDayOptions
                {
                    LocationIdentifiers = new() { "1001" },
                    SelectedDates = new() { firstDate, secondDate },
                    DataSource = dataSource
                }, CancellationToken.None);
            var row = Assert.Single(result.Locations);
            Assert.Equal(2, row.DaysWithData);
            Assert.Equal(4, row.Profile.Points.Single(point => point.Minutes == 480).AverageVolume);
            Assert.Equal(new[] { "Eastbound", "Northbound" }, row.MovementProfiles.Select(profile => profile.Direction));
            Assert.All(row.MovementProfiles, profile => Assert.Equal(4, profile.Points.Single(point => point.Minutes == 480).AverageVolume));
            Assert.Contains(result.Warnings, warning => warning.Code == "CapacityVariesByDate");
            Assert.Null(row.Summary.PeakOccupancyPercent);
        }

        [Fact]
        public async Task ExecuteAsync_MissingConfigurationKeepsValidDatesAndOtherLocationRows()
        {
            var firstDate = new DateOnly(2026, 3, 18);
            var secondDate = firstDate.AddDays(1);
            var secondStart = secondDate.ToDateTime(TimeOnly.MinValue);
            var locations = new Mock<ILocationRepository>();
            locations.Setup(repository => repository.GetLatestVersionOfLocation("1001", secondStart)).Returns(LocationWithDetector());
            var events = EventRepository(new[] { IndianaEvent(secondStart.AddHours(8), IndianaEnumerations.VehicleDetectorOn, 7) });
            var result = await CreateService(events, LocationWithDetector(), locationRepository: locations).ExecuteAsync(new TimeOfDayOptions
            {
                LocationIdentifiers = new() { "1001", "missing" },
                SelectedDates = new() { firstDate, secondDate }
            }, CancellationToken.None);
            var valid = result.Locations.Single(location => location.LocationIdentifier == "1001");
            Assert.Equal("Partial", valid.DataQualityFlag);
            Assert.Equal(new[] { firstDate }, valid.MissingDates);
            Assert.Equal(new[] { secondDate }, valid.DatesWithData);
            Assert.Equal("NoData", result.Locations.Single(location => location.LocationIdentifier == "missing").DataQualityFlag);
            Assert.Equal(3, result.Warnings.Count(warning => warning.Code == "MissingLocationConfiguration"));
            events.Verify(repository => repository.GetEventsBetweenDates("1001", secondStart.AddHours(-12), secondStart.AddDays(1).AddHours(12)), Times.Once);
            events.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        public async Task ExecuteAsync_ResolvesLanesPerDirectionAndFallbackPerApproach(bool detectedLanes, bool overrideEastbound)
        {
            var date = new DateOnly(2026, 3, 18);
            var start = date.ToDateTime(TimeOnly.MinValue);
            var location = LocationWithDetector();
            location.Approaches.Clear();
            foreach (var direction in new[] { DirectionTypes.EB, DirectionTypes.WB, DirectionTypes.NB, DirectionTypes.SB })
            {
                var approach = LocationWithDetector().Approaches.Single();
                approach.Location = location;
                approach.DirectionTypeId = direction;
                if (detectedLanes)
                {
                    approach.Detectors.Single().LaneNumber = 1;
                    approach.Detectors.Add(new Detector { LaneType = LaneTypes.V, LaneNumber = 2 });
                }
                location.Approaches.Add(approach);
            }
            var events = EventRepository(Enumerable.Range(0, 120)
                .Select(index => IndianaEvent(start.AddHours(8).AddSeconds(index), IndianaEnumerations.VehicleDetectorOn, 7)).ToList());
            var options = new TimeOfDayOptions
            {
                LocationIdentifiers = new() { "1001" },
                SelectedDates = new() { date },
                ApproachVolumeAssumedLanes = 2,
                LaneCapacityVehiclesPerHour = 800
            };
            if (overrideEastbound) options.DirectionLaneCounts["Eastbound"] = 2;
            var result = await CreateService(events, location).ExecuteAsync(options, CancellationToken.None);
            // 120 vehicles -> 480 vph -> 160 smoothed vph / (8 lanes * 800) = 2.5%.
            Assert.Equal(2.5, Assert.Single(result.Locations).Summary.PeakOccupancyPercent);
        }

        private static TimeOfDayReportService CreateService(
            Mock<IIndianaEventLogRepository> eventRepository,
            Location location,
            Mock<ISignalTimingPlanRepository>? planRepository = null,
            Mock<IDetectorEventCountAggregationRepository>? detectorAggregationRepository = null,
            Mock<ILocationRepository>? locationRepository = null)
        {
            if (planRepository == null)
            {
                planRepository = new Mock<ISignalTimingPlanRepository>();
                planRepository.Setup(repository => repository.GetList())
                    .Returns(new List<SignalTimingPlan>().AsQueryable());
            }
            if (locationRepository == null)
            {
                locationRepository = new Mock<ILocationRepository>();
                locationRepository.Setup(repository => repository.GetLatestVersionOfLocation(
                    location.LocationIdentifier, It.IsAny<DateTime>())).Returns(location);
            }
            var profileService = new TimeOfDayProfileService();
            var timeOfDayService = new TimeOfDayService(
                new TimeOfDayLocationService(new TimeOfDayObservationService(), profileService),
                profileService,
                new TimeOfDayRecommendationService(profileService),
                new TimeOfDayPlanScheduleService(),
                new TimeOfDayPlanProfileService(),
                new TimeOfDaySplitPressureService(profileService));

            return new TimeOfDayReportService(
                locationRepository.Object,
                eventRepository.Object,
                (detectorAggregationRepository ?? new Mock<IDetectorEventCountAggregationRepository>()).Object,
                planRepository.Object,
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
