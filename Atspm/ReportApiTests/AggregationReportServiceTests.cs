using Utah.Udot.Atspm.Business.Aggregation;
using Utah.Udot.Atspm.Business.Aggregation.FilterExtensions;
using Utah.Udot.Atspm.Business.Bins;
using Utah.Udot.Atspm.Enums;
using Utah.Udot.Atspm.ReportApi.ReportServices;

namespace ReportApiTests
{
    public class AggregationReportServiceTests
    {
        // The checks run before any metric is built, so none of the metric services are needed.
        private static AggregationReportService Service() =>
            new(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);

        private static AggregationOptions ValidOptions() => new()
        {
            LocationIdentifiers = new List<string> { "7115" },
            Locations = new List<FilterSignal> { new() { LocationIdentifier = "7115" } },
            Start = new DateTime(2026, 10, 1),
            End = new DateTime(2026, 10, 15),
            SelectedXAxisType = XAxisType.Time,
            TimeOptions = new TimeOptions
            {
                Start = new DateTime(2026, 10, 1),
                End = new DateTime(2026, 10, 15),
                TimeOption = TimeOptions.TimePeriodOptions.StartToEnd,
                SelectedBinSize = TimeOptions.BinSize.Hour
            }
        };

        private static async Task<string> TurnedAwayWith(AggregationOptions options) =>
            (await Assert.ThrowsAsync<ArgumentException>(() => Service().ExecuteAsync(options, null, default))).Message;

        [Fact]
        public void GetOptionsError_AcceptsTheOptionsThePageSends()
        {
            Assert.Null(AggregationReportService.GetOptionsError(ValidOptions()));
        }

        [Fact]
        public async Task ExecuteAsync_EndNotAfterStart_TurnsTheRequestAway()
        {
            var options = ValidOptions();
            options.End = options.Start;

            Assert.Equal("End must be later than Start.", await TurnedAwayWith(options));
        }

        [Fact]
        public async Task ExecuteAsync_StartAndEndDifferFromTheTimeOptions_TurnsTheRequestAway()
        {
            // The data would be fetched for October and charted in September's time slots.
            var options = ValidOptions();
            options.TimeOptions.Start = new DateTime(2026, 9, 1);
            options.TimeOptions.End = new DateTime(2026, 9, 30);

            Assert.Equal("Start and End must match the time options' Start and End.", await TurnedAwayWith(options));
        }

        [Fact]
        public void GetOptionsError_AcceptsLocationsSentWithoutTheirNumbers()
        {
            // Only the location list is charted, so the list of location numbers is optional.
            var options = ValidOptions();
            options.LocationIdentifiers = null;

            Assert.Null(AggregationReportService.GetOptionsError(options));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteAsync_NoLocations_TurnsTheRequestAway(bool listMissing)
        {
            var options = ValidOptions();
            options.Locations = listMissing ? null : new List<FilterSignal>();

            Assert.Equal("Select at least one location.", await TurnedAwayWith(options));
        }

        [Fact]
        public async Task ExecuteAsync_EmptyLocationEntry_TurnsTheRequestAway()
        {
            var options = ValidOptions();
            options.Locations.Add(null);

            Assert.Equal("Select at least one location.", await TurnedAwayWith(options));
        }

        [Fact]
        public async Task ExecuteAsync_EveryLocationExcluded_TurnsTheRequestAway()
        {
            var options = ValidOptions();
            options.Locations[0].Exclude = true;

            Assert.Equal("All of the selected locations are excluded.", await TurnedAwayWith(options));
        }

        [Fact]
        public async Task ExecuteAsync_TimeOfDayOverTheWholeRange_ChecksTheChosenDays()
        {
            // Thursday October 1 up to Saturday October 3, with only Mondays chosen.
            var options = ValidOptions();
            options.End = options.TimeOptions.End = new DateTime(2026, 10, 3);
            options.SelectedXAxisType = XAxisType.TimeOfDay;
            options.TimeOptions.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

            Assert.Equal("None of the chosen days fall in the date range.", await TurnedAwayWith(options));
        }
    }
}
