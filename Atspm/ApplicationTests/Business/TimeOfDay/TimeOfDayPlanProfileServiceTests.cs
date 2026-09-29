#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay/TimeOfDayPlanProfileServiceTests.cs
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

using System.Collections.Generic;
using System.Linq;
using Utah.Udot.Atspm.Business.TimeOfDay;
using Xunit;

namespace Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay
{
    public class TimeOfDayPlanProfileServiceTests
    {
        [Fact]
        public void BuildPlanProfile_EmitsAmAndPmLocationPeaksForEachLocation()
        {
            var result = new TimeOfDayPlanProfileService().BuildPlanProfile(
                new TimeOfDayProfileDto(),
                new List<TimeOfDayProfileDto>(),
                new List<TimeOfDayLocationResult>
                {
                    BuildLocation("1001", amPeakVolume: 300, pmPeakVolume: 700),
                    BuildLocation("1002", amPeakVolume: 500, pmPeakVolume: 400)
                });

            var locationPeaks = result.Peaks
                .Where(p => p.Series == "Location")
                .ToList();

            Assert.Equal(4, locationPeaks.Count);
            Assert.All(new[] { "1001", "1002" }, locationIdentifier =>
            {
                Assert.Contains(locationPeaks, p => p.LocationIdentifier == locationIdentifier && p.Period == "AM");
                Assert.Contains(locationPeaks, p => p.LocationIdentifier == locationIdentifier && p.Period == "PM");
            });
            Assert.Contains(locationPeaks, p => p.LocationIdentifier == "1001" && p.Period == "AM" && p.Minutes == 480);
            Assert.Contains(locationPeaks, p => p.LocationIdentifier == "1001" && p.Period == "PM" && p.Minutes == 960);
        }

        [Fact]
        public void BuildPlanProfile_SkipsLocationPeriodPeaksWithNoVolume()
        {
            var result = new TimeOfDayPlanProfileService().BuildPlanProfile(
                new TimeOfDayProfileDto(),
                new List<TimeOfDayProfileDto>(),
                new List<TimeOfDayLocationResult>
                {
                    BuildLocation("1001", amPeakVolume: 0, pmPeakVolume: 700)
                });

            var locationPeaks = result.Peaks
                .Where(p => p.Series == "Location")
                .ToList();

            Assert.DoesNotContain(locationPeaks, p => p.Period == "AM");
            Assert.Contains(locationPeaks, p => p.Period == "PM");
        }

        private static TimeOfDayLocationResult BuildLocation(
            string locationIdentifier,
            double amPeakVolume,
            double pmPeakVolume)
        {
            return new TimeOfDayLocationResult
            {
                LocationIdentifier = locationIdentifier,
                LocationDescription = locationIdentifier,
                Profile = new TimeOfDayProfileDto
                {
                    Units = "vph",
                    Points = new List<TimeOfDayProfilePointDto>
                    {
                        BuildPoint(480, amPeakVolume),
                        BuildPoint(960, pmPeakVolume)
                    }
                }
            };
        }

        private static TimeOfDayProfilePointDto BuildPoint(int minutes, double volume)
        {
            return new TimeOfDayProfilePointDto
            {
                TimeOfDay = $"{minutes / 60:00}:{minutes % 60:00}",
                Minutes = minutes,
                AverageVolume = volume,
                SmoothedVolume = volume
            };
        }
    }
}
