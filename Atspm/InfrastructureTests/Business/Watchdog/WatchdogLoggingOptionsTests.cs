#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.ATSPM.InfrastructureTests.Business.Watchdog/WatchdogLoggingOptionsTests.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using Utah.Udot.Atspm.Business.Watchdog;
using Xunit;

namespace Utah.Udot.ATSPM.InfrastructureTests.Business.Watchdog
{
    public class WatchdogLoggingOptionsTests
    {
        [Fact]
        public void RampDetectorEnd_Should_Use_EndScanDate()
        {
            var options = new WatchdogRampLoggingOptions
            {
                RampMissedDetectorHitsStartScanDate = new DateTime(2026, 4, 24),
                RampMissedDetectorHitsEndScanDate = new DateTime(2026, 4, 25),
                RampDetectorEndHour = 19
            };

            Assert.Equal(new DateTime(2026, 4, 25, 19, 0, 0), options.RampDetectorEnd);
        }
    }
}
