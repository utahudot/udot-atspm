using Utah.Udot.Atspm.Business.TimeOfDay;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace ReportApiTests
{
    public class TimeOfDayProfileServiceTests
    {
        private readonly TimeOfDayProfileService service = new();
        private readonly TimeOfDayObservationService observationService = new();

        [Fact]
        public void BuildProfile_ReturnsNinetySixPointsForFifteenMinuteProfile()
        {
            var result = service.BuildProfile(
                "Corridor",
                string.Empty,
                string.Empty,
                string.Empty,
                new List<TimeOfDayVolumeObservation>(),
                new List<DateOnly> { new(2026, 3, 18) },
                15);

            Assert.Equal(96, result.Points.Count);
            Assert.Equal("00:00", result.Points[0].TimeOfDay);
            Assert.Equal("23:45", result.Points[^1].TimeOfDay);
        }

        [Fact]
        public void BuildProfile_AveragesMultipleSelectedDatesIntoRepresentativeBin()
        {
            var observations = new List<TimeOfDayVolumeObservation>
            {
                Observation(new DateOnly(2026, 3, 18), 8 * 60),
                Observation(new DateOnly(2026, 3, 18), 8 * 60),
                Observation(new DateOnly(2026, 3, 19), 8 * 60)
            };

            var result = service.BuildProfile(
                "Corridor",
                string.Empty,
                string.Empty,
                string.Empty,
                observations,
                new List<DateOnly> { new(2026, 3, 18), new(2026, 3, 19) },
                15);

            var point = result.Points.Single(p => p.Minutes == 8 * 60);
            Assert.Equal(6, point.AverageVolume);
        }

        [Fact]
        public void BuildProfile_UsesCenteredThreeBinSmoothing()
        {
            var observations = new List<TimeOfDayVolumeObservation>
            {
                Observation(new DateOnly(2026, 3, 18), 0),
                Observation(new DateOnly(2026, 3, 18), 15),
                Observation(new DateOnly(2026, 3, 18), 15),
                Observation(new DateOnly(2026, 3, 18), 30),
                Observation(new DateOnly(2026, 3, 18), 30),
                Observation(new DateOnly(2026, 3, 18), 30)
            };

            var result = service.BuildProfile(
                "Corridor",
                string.Empty,
                string.Empty,
                string.Empty,
                observations,
                new List<DateOnly> { new(2026, 3, 18) },
                15);

            Assert.Equal(8, result.Points.Single(p => p.Minutes == 15).SmoothedVolume);
            Assert.Equal(6, result.Points.Single(p => p.Minutes == 0).SmoothedVolume);
        }

        [Fact]
        public void BuildProfile_UsesTrailingFourBinRollingHourVph()
        {
            var observations = new List<TimeOfDayVolumeObservation>
            {
                Observation(new DateOnly(2026, 3, 18), 0),
                Observation(new DateOnly(2026, 3, 18), 15),
                Observation(new DateOnly(2026, 3, 18), 30),
                Observation(new DateOnly(2026, 3, 18), 45)
            };

            var result = service.BuildProfile(
                "Corridor",
                string.Empty,
                string.Empty,
                string.Empty,
                observations,
                new List<DateOnly> { new(2026, 3, 18) },
                15);

            Assert.Null(result.Points.Single(p => p.Minutes == 30).RollingHourVph);
            Assert.Equal(4, result.Points.Single(p => p.Minutes == 45).RollingHourVph);
        }

        [Fact]
        public void BuildIndianaEventObservations_FiltersAndBinsDetectorEvents()
        {
            var selectedDate = new DateOnly(2026, 3, 18);
            var location = LocationWithDetector(7, DirectionTypes.NB, MovementTypes.T);
            var events = new List<IndianaEvent>
            {
                IndianaEvent(82, 7, new DateTime(2026, 3, 18, 8, 7, 0)),
                IndianaEvent(81, 7, new DateTime(2026, 3, 18, 8, 7, 0)),
                IndianaEvent(82, 99, new DateTime(2026, 3, 18, 8, 7, 0))
            };

            var result = observationService.BuildIndianaEventObservations(
                location,
                "Location",
                new List<DateOnly> { selectedDate },
                15,
                events);

            var observation = Assert.Single(result.Observations);
            Assert.True(result.HasEligibleDetectors);
            Assert.Equal(8 * 60, observation.Minutes);
            Assert.Equal("Northbound", observation.Direction);
            Assert.Equal("Thru", observation.MovementLabel);
        }

        [Fact]
        public void BuildIndianaEventObservations_ExcludesEventsCorrectedOutsideSelectedDate()
        {
            var selectedDate = new DateOnly(2026, 3, 18);
            var location = LocationWithDetector(7, DirectionTypes.NB, MovementTypes.T, latencyCorrection: 2);
            var events = new List<IndianaEvent>
            {
                IndianaEvent(82, 7, new DateTime(2026, 3, 18, 0, 0, 1))
            };

            var result = observationService.BuildIndianaEventObservations(
                location,
                "Location",
                new List<DateOnly> { selectedDate },
                15,
                events);

            Assert.True(result.HasEligibleDetectors);
            Assert.Empty(result.Observations);
        }

        [Fact]
        public void BuildIndianaEventObservations_DeduplicatesIdenticalEvents()
        {
            var selectedDate = new DateOnly(2026, 3, 18);
            var location = LocationWithDetector(7, DirectionTypes.NB, MovementTypes.T);
            var detectorEvent = IndianaEvent(82, 7, new DateTime(2026, 3, 18, 23, 30, 0));

            var result = observationService.BuildIndianaEventObservations(
                location,
                "Location",
                new List<DateOnly> { selectedDate },
                15,
                new List<IndianaEvent> { detectorEvent, IndianaEvent(82, 7, detectorEvent.Timestamp) });

            Assert.Single(result.Observations);
        }

        private static TimeOfDayVolumeObservation Observation(DateOnly date, int minutes)
        {
            return new TimeOfDayVolumeObservation(
                "1001",
                "Location",
                date,
                minutes,
                "Northbound",
                "Thru",
                "Thru",
                1);
        }

        private static IndianaEvent IndianaEvent(short eventCode, short eventParam, DateTime timestamp)
        {
            return new IndianaEvent
            {
                LocationIdentifier = "1001",
                EventCode = eventCode,
                EventParam = eventParam,
                Timestamp = timestamp
            };
        }

        private static Location LocationWithDetector(
            int detectorChannel,
            DirectionTypes direction,
            MovementTypes movement,
            double latencyCorrection = 0)
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
                DirectionTypeId = direction,
                Mph = 35
            };
            var detector = new Detector
            {
                Id = detectorChannel,
                Approach = approach,
                DetectorChannel = detectorChannel,
                LaneType = LaneTypes.V,
                MovementType = movement,
                DistanceFromStopBar = 0,
                DecisionPoint = 0,
                LatencyCorrection = latencyCorrection,
                DetectionTypes = new List<DetectionType>
                {
                    new()
                    {
                        Id = DetectionTypes.LLC,
                        MeasureTypes = new List<MeasureType>
                        {
                            new() { Id = 5 }
                        }
                    }
                }
            };

            approach.Detectors = new List<Detector> { detector };
            location.Approaches = new List<Approach> { approach };
            return location;
        }
    }
}
