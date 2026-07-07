using Utah.Udot.Atspm.Infrastructure.Configuration;
using Xunit;

namespace Utah.Udot.ATSPM.WatchDogTests.Configuration
{
    public class WatchdogConfigurationTests
    {
        [Fact]
        public void AutomaticScanDatesUseConfiguredTimeZone()
        {
            var timeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 7, 2, 2, 0, 0, TimeSpan.Zero));
            var configuration = new WatchdogConfiguration
            {
                TimeZoneId = "America/Denver"
            };

            Assert.Equal(new DateTime(2026, 6, 30), configuration.GetPmScanDate(timeProvider));
            Assert.Equal(new DateTime(2026, 7, 1), configuration.GetAmScanDate(timeProvider));
            Assert.Equal(new DateTime(2026, 6, 30), configuration.GetRampMissedDetectorHitsStartScanDate(timeProvider));
            Assert.Equal(new DateTime(2026, 7, 1), configuration.GetRampMissedDetectorHitsEndScanDate(timeProvider));
        }

        [Fact]
        public void ExplicitScanDatesArePreserved()
        {
            var timeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 7, 2, 2, 0, 0, TimeSpan.Zero));
            var configuration = new WatchdogConfiguration
            {
                TimeZoneId = "America/Denver",
                PmScanDate = new DateTime(2026, 4, 24, 13, 30, 0),
                AmScanDate = new DateTime(2026, 4, 25, 6, 15, 0),
                RampMissedDetectorHitsStartScanDate = new DateTime(2026, 4, 26, 8, 45, 0),
                RampMissedDetectorHitsEndScanDate = new DateTime(2026, 4, 27, 9, 0, 0)
            };

            Assert.Equal(new DateTime(2026, 4, 24), configuration.GetPmScanDate(timeProvider));
            Assert.Equal(new DateTime(2026, 4, 25), configuration.GetAmScanDate(timeProvider));
            Assert.Equal(new DateTime(2026, 4, 26), configuration.GetRampMissedDetectorHitsStartScanDate(timeProvider));
            Assert.Equal(new DateTime(2026, 4, 27), configuration.GetRampMissedDetectorHitsEndScanDate(timeProvider));
        }

        private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => utcNow;
        }
    }
}
