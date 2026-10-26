using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Utah.Udot.Atspm.Business.Aggregation;
using Utah.Udot.Atspm.Business.Aggregation.FilterExtensions;
using Utah.Udot.Atspm.Business.Bins;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Enums;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.ReportApi.DataAggregation;

namespace ReportApiTests
{
    public class SignalAggregationMetricOptionsTests
    {
        // Only the shared chart steps are under test, so the per-metric bins are never built.
        private class TestMetricOptions : SignalAggregationMetricOptions
        {
            public TestMetricOptions(ILocationRepository locationRepository) : base(locationRepository, NullLogger.Instance) { }

            public List<Location> FindLocations(AggregationOptions options) => GetSignalObjects(options);

            protected override List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options) =>
                throw new NotImplementedException();

            public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options) =>
                throw new NotImplementedException();
        }

        private static Mock<ILocationRepository> RepositoryWith(params Location[] locations)
        {
            var locationRepository = new Mock<ILocationRepository>();
            locationRepository
                .Setup(r => r.GetLatestVersionOfLocation(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns((string identifier, DateTime _) =>
                    locations.FirstOrDefault(location => location.LocationIdentifier == identifier));
            return locationRepository;
        }

        private static TestMetricOptions WithLocations(params Location[] locations) =>
            new(RepositoryWith(locations).Object);

        private static AggregationOptions ForLocation(bool exclude) => new()
        {
            LocationIdentifiers = new List<string> { "7115" },
            Locations = new List<FilterSignal> { new() { LocationIdentifier = "7115", Exclude = exclude } },
            TimeOptions = new TimeOptions { Start = new DateTime(2026, 10, 1), End = new DateTime(2026, 10, 15) },
            FilterDirections = new List<FilterDirection>(),
            FilterMovements = new List<FilterMovement>()
        };

        [Fact]
        public void ForLocations_TagsAChartWithItsOneLocation()
        {
            var chart = SignalAggregationMetricOptions.ForLocations(new AggregationResult(), new Location { LocationIdentifier = "7116" });

            Assert.Equal(new[] { "7116" }, chart.LocationIdentifiers);
        }

        [Fact]
        public void ForLocations_TagsACombinedChartWithEachLocationOnceInOrder()
        {
            var locations = new[]
            {
                new Location { LocationIdentifier = "7115" },
                new Location { LocationIdentifier = "7116" },
                new Location { LocationIdentifier = "7115" },
                new Location { LocationIdentifier = "7117" },
            };

            var chart = SignalAggregationMetricOptions.ForLocations(new AggregationResult(), locations);

            Assert.Equal(new[] { "7115", "7116", "7117" }, chart.LocationIdentifiers);
        }

        [Fact]
        public void GetSignalObjects_ReturnsTheLocationsFound()
        {
            var location = new Location { LocationIdentifier = "7115", Approaches = new List<Approach>() };

            var locations = WithLocations(location).FindLocations(ForLocation(exclude: false));

            Assert.Equal(new[] { location }, locations);
        }

        [Fact]
        public void GetSignalObjects_UsesEachLocationsVersionInEffectOnStart()
        {
            var location = new Location { LocationIdentifier = "7115", Approaches = new List<Approach>() };
            var locationRepository = RepositoryWith(location);

            new TestMetricOptions(locationRepository.Object).FindLocations(ForLocation(exclude: false));

            locationRepository.Verify(r => r.GetLatestVersionOfLocation("7115", new DateTime(2026, 10, 1)), Times.Once);
            locationRepository.Verify(
                r => r.GetLocationsBetweenDates(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public void GetSignalObjects_NoLocationFoundForTheDates_TurnsTheRequestAway()
        {
            var error = Assert.Throws<ArgumentException>(() => WithLocations().FindLocations(ForLocation(exclude: false)));

            Assert.Equal("None of the selected locations were found for the chosen Start and End.", error.Message);
        }

        [Fact]
        public void GetSignalObjects_SomeLocationsFound_ChartsTheOnesFound()
        {
            var location = new Location { LocationIdentifier = "7115", Approaches = new List<Approach>() };
            var options = ForLocation(exclude: false);
            options.LocationIdentifiers.Add("7116");
            options.Locations.Add(new FilterSignal { LocationIdentifier = "7116" });

            var locations = WithLocations(location).FindLocations(options);

            Assert.Equal(new[] { location }, locations);
        }

        [Fact]
        public void GetSignalObjects_NoFilterLists_KeepsEveryApproachAndDetector()
        {
            // The direction and movement filters are optional for callers other than the page.
            var location = VersionOf7115(100, ApproachOf(1, DirectionTypes.NB, 2, new Detector { Id = 10 }));
            var options = ForLocation(exclude: false);
            options.FilterDirections = null;
            options.FilterMovements = null;

            var found = WithLocations(location).FindLocations(options).Single();

            Assert.Equal(new[] { 10 }, found.Approaches.Single().Detectors.Select(detector => detector.Id));
        }

        [Fact]
        public void GetFilteredDirections_NoDirectionFilters_ChartsEveryDirection()
        {
            var options = ForLocation(exclude: false);
            options.FilterDirections = null;

            var directions = WithLocations().GetFilteredDirections(options);

            Assert.Equal(8, directions.Count);
            Assert.DoesNotContain(DirectionTypes.NA, directions);
        }

        private static Approach ApproachOf(int id, DirectionTypes direction, int phase, params Detector[] detectors)
        {
            var approach = new Approach { Id = id, DirectionTypeId = direction, ProtectedPhaseNumber = phase };
            foreach (var detector in detectors)
                approach.Detectors.Add(detector);
            return approach;
        }

        private static Location VersionOf7115(int id, params Approach[] approaches)
        {
            var location = new Location { Id = id, LocationIdentifier = "7115" };
            foreach (var approach in approaches)
                location.Approaches.Add(approach);
            return location;
        }

        private static AggregationOptions Excluding(params FilterApproach[] approaches)
        {
            var options = ForLocation(exclude: false);
            options.Locations[0].Approaches = approaches.ToList();
            return options;
        }

        [Fact]
        public void GetSignalObjects_ExcludedApproach_IsLeftOut()
        {
            var location = VersionOf7115(100, ApproachOf(1, DirectionTypes.NB, 2), ApproachOf(2, DirectionTypes.SB, 6));
            var locationRepository = RepositoryWith(location);

            var found = new TestMetricOptions(locationRepository.Object)
                .FindLocations(Excluding(new FilterApproach { ApproachId = 1, Exclude = true }))
                .Single();

            Assert.Equal(new[] { 2 }, found.Approaches.Select(approach => approach.Id));
            locationRepository.Verify(r => r.GetLatestVersionOfLocation(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetSignalObjects_ExcludedDetector_IsLeftOutAndItsApproachKept()
        {
            var location = VersionOf7115(100, ApproachOf(1, DirectionTypes.NB, 2,
                new Detector { Id = 10, DetectorChannel = 5 }, new Detector { Id = 11, DetectorChannel = 6 }));
            var exclusion = new FilterApproach { ApproachId = 1, Detectors = { new FilterDetector { Id = 10, Exclude = true } } };

            var found = WithLocations(location).FindLocations(Excluding(exclusion)).Single();

            Assert.Equal(new[] { 11 }, found.Approaches.Single().Detectors.Select(detector => detector.Id));
        }

        [Fact]
        public void GetSignalObjects_ExclusionsPickedOnTheLatestVersion_MatchTheVersionInEffectOnStart()
        {
            // The page lists the latest version of 7115, but the chart uses the one in effect on Start.
            var onStart = VersionOf7115(100,
                ApproachOf(1, DirectionTypes.NB, 2,
                    new Detector { Id = 10, DetectorChannel = 5 }, new Detector { Id = 11, DetectorChannel = 6 }),
                ApproachOf(2, DirectionTypes.SB, 6));
            var latest = VersionOf7115(200,
                ApproachOf(201, DirectionTypes.NB, 2,
                    new Detector { Id = 210, DetectorChannel = 5 }, new Detector { Id = 211, DetectorChannel = 6 }),
                ApproachOf(202, DirectionTypes.SB, 6));
            var locationRepository = RepositoryWith(onStart);
            locationRepository.Setup(r => r.GetLatestVersionOfLocation("7115")).Returns(latest);
            var northbound = new FilterApproach { ApproachId = 201, Detectors = { new FilterDetector { Id = 210, Exclude = true } } };
            var southbound = new FilterApproach { ApproachId = 202, Exclude = true };

            var found = new TestMetricOptions(locationRepository.Object)
                .FindLocations(Excluding(northbound, southbound))
                .Single();

            Assert.Equal(new[] { 1 }, found.Approaches.Select(approach => approach.Id));
            Assert.Equal(new[] { 11 }, found.Approaches.Single().Detectors.Select(detector => detector.Id));
        }

        [Fact]
        public void SetTimeAggregateSeries_TimePeriodEndingAtMidnight_ChartsToTheEndOfTheDay()
        {
            var options = ForLocation(exclude: false);
            options.SelectedXAxisType = XAxisType.TimeOfDay;
            options.SelectedAggregationType = AggregationCalculationType.Sum;
            options.TimeOptions = new TimeOptions
            {
                Start = new DateTime(2026, 10, 1),
                End = new DateTime(2026, 10, 3),
                TimeOfDayStartHour = 22,
                TimeOfDayStartMinute = 0,
                TimeOfDayEndHour = 0,
                TimeOfDayEndMinute = 0,
                TimeOption = TimeOptions.TimePeriodOptions.TimePeriod,
                SelectedBinSize = TimeOptions.BinSize.Hour
            };
            var series = new Series();

            WithLocations().SetTimeAggregateSeries(series, BinFactory.GetBins(options.TimeOptions), options);

            Assert.Equal(
                new[] { new DateTime(2026, 10, 1, 22, 0, 0), new DateTime(2026, 10, 1, 23, 0, 0) },
                series.DataPoints.Select(point => point.Start));
        }

        [Fact]
        public void SetTimeAggregateSeries_DayBins_ChartOnlyDaysThatStartBeforeEnd()
        {
            var options = ForLocation(exclude: false);
            options.SelectedXAxisType = XAxisType.TimeOfDay;
            options.SelectedAggregationType = AggregationCalculationType.Sum;
            options.TimeOptions = new TimeOptions
            {
                Start = new DateTime(2026, 10, 1),
                End = new DateTime(2026, 10, 3),
                TimeOfDayStartHour = 22,
                TimeOfDayStartMinute = 0,
                TimeOfDayEndHour = 0,
                TimeOfDayEndMinute = 0,
                TimeOption = TimeOptions.TimePeriodOptions.TimePeriod,
                SelectedBinSize = TimeOptions.BinSize.Day
            };
            var series = new Series();

            WithLocations().SetTimeAggregateSeries(series, BinFactory.GetBins(options.TimeOptions), options);

            Assert.Equal(
                new[] { new DateTime(2026, 10, 1), new DateTime(2026, 10, 2) },
                series.DataPoints.Select(point => point.Start));
        }
    }
}
