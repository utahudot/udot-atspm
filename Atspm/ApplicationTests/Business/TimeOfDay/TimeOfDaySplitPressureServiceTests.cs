#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay/TimeOfDaySplitPressureServiceTests.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Utah.Udot.Atspm.Business.TimeOfDay;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Xunit;

namespace Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay
{
    public class TimeOfDaySplitPressureServiceTests
    {
        private static readonly DateOnly TestDate = new(2026, 1, 1);

        [Fact]
        public void BuildSplitPressure_UsesMedianRepresentativeProfilesAcrossLocations()
        {
            var service = CreateService();
            var result = service.BuildSplitPressure(
                new TimeOfDayOptions
                {
                    AllDayPrimaryDirections = new List<string> { "Eastbound" }
                },
                BuildDirectionalProfiles(),
                new List<TimeOfDayLocationAnalysisData>
                {
                    BuildLocation("1001", 25, 10),
                    BuildLocation("1002", 75, 20),
                    BuildLocation("1003", 225, 30)
                },
                new List<DateOnly> { TestDate },
                15);

            var primaryPoint = result.PrimaryProfile.Points.Single(p => p.Minutes == 480);
            var crossPoint = result.CrossStreetProfile.Points.Single(p => p.Minutes == 480);
            var sharePoint = result.CrossTrafficShare.Single(p => p.Minutes == 480);

            Assert.Equal(300, primaryPoint.AverageVolume);
            Assert.Equal(80, crossPoint.AverageVolume);
            Assert.Equal(300, result.PrimaryPeakVolume);
            Assert.Equal(80, result.CrossStreetPeakVolume);
            Assert.Equal(21.05, sharePoint.CrossTrafficPercent);

            var locationShares = result.CrossTrafficLocations.Where(row => row.Period == "AM").ToList();
            Assert.Equal(28.57, locationShares.Single(row => row.LocationIdentifier == "1001").PercentOfCrossTraffic);
            Assert.Equal(21.05, locationShares.Single(row => row.LocationIdentifier == "1002").PercentOfCrossTraffic);
            Assert.Equal(11.76, locationShares.Single(row => row.LocationIdentifier == "1003").PercentOfCrossTraffic);
            Assert.All(locationShares, row => Assert.InRange(row.PercentOfCrossTraffic!.Value, 0, 100));
        }

        [Theory]
        [InlineData(125, 125, 50)]
        [InlineData(0, 125, 100)]
        public void BuildSplitPressure_UsesLocationTotalAtCrossTrafficPeak(
            double primaryCount,
            double crossCount,
            double expectedPercent)
        {
            var location = BuildLocation("1001", primaryCount, crossCount);
            // A larger primary peak in another bin must not affect this share.
            location.Observations.Add(BuildObservation("1001", "Eastbound", 1000) with { Minutes = 495 });

            var result = CreateService().BuildSplitPressure(
                new TimeOfDayOptions { AllDayPrimaryDirections = new List<string> { "Eastbound" } },
                BuildDirectionalProfiles(),
                new List<TimeOfDayLocationAnalysisData> { location },
                new List<DateOnly> { TestDate },
                15);

            var row = Assert.Single(result.CrossTrafficLocations.Where(row => row.Period == "AM"));
            Assert.Equal(480, row.Minutes);
            Assert.Equal(500, row.TotalVehiclesPerHour);
            Assert.Equal(expectedPercent, row.PercentOfCrossTraffic);
        }

        [Fact]
        public void BuildSplitPressure_UsesCommonDateBasisForLocationShare()
        {
            var location = BuildLocation("1001", 125, 125);
            // Primary traffic has another day of data; cross traffic does not.
            location.Observations.Add(BuildObservation("1001", "Eastbound", 250) with { LocalDate = TestDate.AddDays(1) });

            var result = CreateService().BuildSplitPressure(
                new TimeOfDayOptions { AllDayPrimaryDirections = new List<string> { "Eastbound" } },
                BuildDirectionalProfiles(),
                new List<TimeOfDayLocationAnalysisData> { location },
                new List<DateOnly> { TestDate, TestDate.AddDays(1) },
                15);

            var row = Assert.Single(result.CrossTrafficLocations.Where(row => row.Period == "AM"));
            Assert.Equal(25, row.PercentOfCrossTraffic);
        }

        [Fact]
        public void BuildSplitPressure_SuppressesCrossTrafficShareBelowLowVolumeFloor()
        {
            var service = CreateService();
            var result = service.BuildSplitPressure(
                new TimeOfDayOptions
                {
                    AllDayPrimaryDirections = new List<string> { "Eastbound" }
                },
                new List<TimeOfDayProfileDto>
                {
                    BuildProfile("Eastbound", "Eastbound", 480, 100),
                    BuildProfile("Northbound", "Northbound", 480, 50)
                },
                new List<TimeOfDayLocationAnalysisData>(),
                new List<DateOnly> { TestDate },
                15);

            var share = Assert.Single(result.CrossTrafficShare);
            Assert.Equal(100, share.PrimaryVolume);
            Assert.Equal(50, share.CrossStreetVolume);
            Assert.Equal(150, share.TotalVolume);
            Assert.Null(share.CrossTrafficPercent);
        }

        [Fact]
        public void BuildSplitPressure_UsesOnlyAllDayPrimaryDirectionsForSplitPressure()
        {
            var service = CreateService();
            var result = service.BuildSplitPressure(
                new TimeOfDayOptions
                {
                    AllDayPrimaryDirections = new List<string> { "Eastbound" },
                    AmPrimaryDirections = new List<string> { "Northbound" },
                    PmPrimaryDirections = new List<string> { "Northbound" }
                },
                new List<TimeOfDayProfileDto>
                {
                    BuildProfile("Eastbound", "Eastbound", 480, 600),
                    BuildProfile("Northbound", "Northbound", 480, 400)
                },
                new List<TimeOfDayLocationAnalysisData>(),
                new List<DateOnly> { TestDate },
                15);

            Assert.Equal(new[] { "Eastbound" }, result.PrimaryDirections);
            Assert.Equal(new[] { "Northbound" }, result.CrossDirections);
        }

        [Fact]
        public void BuildSplitPressure_InfersCrossDirectionsFromOppositeAxis()
        {
            var service = CreateService();
            var result = service.BuildSplitPressure(
                new TimeOfDayOptions
                {
                    AllDayPrimaryDirections = new List<string> { "Eastbound" }
                },
                new List<TimeOfDayProfileDto>
                {
                    BuildProfile("Eastbound", "Eastbound", 480, 600),
                    BuildProfile("Westbound", "Westbound", 480, 500),
                    BuildProfile("Northbound", "Northbound", 480, 300),
                    BuildProfile("Southbound", "Southbound", 480, 200)
                },
                new List<TimeOfDayLocationAnalysisData>(),
                new List<DateOnly> { TestDate },
                15);

            Assert.Equal(new[] { "Northbound", "Southbound" }, result.CrossDirections);
        }

        [Fact]
        public void BuildSplitPressure_AlignsCrossTrafficShareByMinute()
        {
            var service = CreateService();
            var result = service.BuildSplitPressure(
                new TimeOfDayOptions
                {
                    AllDayPrimaryDirections = new List<string> { "Eastbound" }
                },
                new List<TimeOfDayProfileDto>
                {
                    BuildProfile("Eastbound", "Eastbound", 480, 1000),
                    BuildProfile("Northbound", "Northbound", 495, 500)
                },
                new List<TimeOfDayLocationAnalysisData>(),
                new List<DateOnly> { TestDate },
                15);

            var primaryOnly = result.CrossTrafficShare.Single(p => p.Minutes == 480);
            var crossOnly = result.CrossTrafficShare.Single(p => p.Minutes == 495);

            Assert.Equal(1000, primaryOnly.PrimaryVolume);
            Assert.Equal(0, primaryOnly.CrossStreetVolume);
            Assert.Equal(0, primaryOnly.CrossTrafficPercent);
            Assert.Equal(0, crossOnly.PrimaryVolume);
            Assert.Equal(500, crossOnly.CrossStreetVolume);
            Assert.Equal(100, crossOnly.CrossTrafficPercent);
        }

        [Fact]
        public void BuildSplitPressure_SuppressesResultsWhenExplicitPrimaryDirectionIsMissing()
        {
            var result = CreateService().BuildSplitPressure(
                new TimeOfDayOptions
                {
                    AllDayPrimaryDirections = new List<string> { "Eastbound" }
                },
                new List<TimeOfDayProfileDto>
                {
                    BuildProfile("Northbound", "Northbound", 480, 500)
                },
                new List<TimeOfDayLocationAnalysisData>(),
                new List<DateOnly> { TestDate },
                15);

            Assert.Empty(result.CrossTrafficShare);
            Assert.Empty(result.MovementPressures);
            Assert.Null(result.PeakCrossTrafficPercent);
            Assert.Empty(result.ReviewText);
            Assert.Contains("Eastbound", result.SummaryText);
        }

        private static TimeOfDaySplitPressureService CreateService()
        {
            return new TimeOfDaySplitPressureService(new TimeOfDayProfileService());
        }

        private static List<TimeOfDayProfileDto> BuildDirectionalProfiles()
        {
            return new List<TimeOfDayProfileDto>
            {
                BuildProfile("Eastbound", "Eastbound", 480, 0),
                BuildProfile("Northbound", "Northbound", 480, 0)
            };
        }

        private static TimeOfDayLocationAnalysisData BuildLocation(
            string locationIdentifier,
            double primaryCount,
            double crossCount)
        {
            return new TimeOfDayLocationAnalysisData
            {
                Location = new Location { LocationIdentifier = locationIdentifier },
                Observations = new List<TimeOfDayVolumeObservation>
                {
                    BuildObservation(locationIdentifier, "Eastbound", primaryCount),
                    BuildObservation(locationIdentifier, "Northbound", crossCount)
                }
            };
        }

        private static TimeOfDayVolumeObservation BuildObservation(
            string locationIdentifier,
            string direction,
            double count)
        {
            return new TimeOfDayVolumeObservation(
                locationIdentifier,
                locationIdentifier,
                TestDate,
                480,
                direction,
                "Thru",
                "Thru",
                count);
        }

        private static TimeOfDayProfileDto BuildProfile(
            string label,
            string direction,
            int minutes,
            double averageVolume)
        {
            return new TimeOfDayProfileDto
            {
                Label = label,
                Direction = direction,
                Points = new List<TimeOfDayProfilePointDto>
                {
                    new()
                    {
                        TimeOfDay = "08:00",
                        Minutes = minutes,
                        AverageVolume = averageVolume,
                        SmoothedVolume = averageVolume
                    }
                }
            };
        }
    }
}
