#region license
// Copyright 2026 Utah Departement of Transportation
// for WatchDogTests - Utah.Udot.ATSPM.WatchDogTests.Configuration/WatchdogConfigurationTests.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

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
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetPmScanDate(timeProvider).Kind);
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetAmScanDate(timeProvider).Kind);
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetRampMissedDetectorHitsStartScanDate(timeProvider).Kind);
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetRampMissedDetectorHitsEndScanDate(timeProvider).Kind);
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
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetPmScanDate(timeProvider).Kind);
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetAmScanDate(timeProvider).Kind);
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetRampMissedDetectorHitsStartScanDate(timeProvider).Kind);
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetRampMissedDetectorHitsEndScanDate(timeProvider).Kind);
        }

        [Fact]
        public void ExplicitScanDatesAreNormalizedToUnspecifiedKind()
        {
            var timeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 7, 2, 2, 0, 0, TimeSpan.Zero));
            var configuration = new WatchdogConfiguration
            {
                TimeZoneId = "America/Denver",
                PmScanDate = new DateTime(2026, 4, 24, 13, 30, 0, DateTimeKind.Utc),
                AmScanDate = new DateTime(2026, 4, 25, 6, 15, 0, DateTimeKind.Local),
                RampMissedDetectorHitsStartScanDate = new DateTime(2026, 4, 26, 8, 45, 0, DateTimeKind.Utc),
                RampMissedDetectorHitsEndScanDate = new DateTime(2026, 4, 27, 9, 0, 0, DateTimeKind.Local)
            };

            Assert.Equal(new DateTime(2026, 4, 24), configuration.GetPmScanDate(timeProvider));
            Assert.Equal(new DateTime(2026, 4, 25), configuration.GetAmScanDate(timeProvider));
            Assert.Equal(new DateTime(2026, 4, 26), configuration.GetRampMissedDetectorHitsStartScanDate(timeProvider));
            Assert.Equal(new DateTime(2026, 4, 27), configuration.GetRampMissedDetectorHitsEndScanDate(timeProvider));
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetPmScanDate(timeProvider).Kind);
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetAmScanDate(timeProvider).Kind);
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetRampMissedDetectorHitsStartScanDate(timeProvider).Kind);
            Assert.Equal(DateTimeKind.Unspecified, configuration.GetRampMissedDetectorHitsEndScanDate(timeProvider).Kind);
        }

        private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => utcNow;
        }
    }
}
