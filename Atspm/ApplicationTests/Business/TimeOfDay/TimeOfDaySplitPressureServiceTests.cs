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
            Assert.Equal(250, row.TotalVehiclesPerHour);
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
        public void BuildSplitPressure_UsesAmDirectionsForAmProfile()
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
            Assert.Equal(new[] { "Northbound" }, result.PrimaryDirectionsByPeriod["AM"]);
            var share = Assert.Single(result.CrossTrafficShare);
            Assert.Equal(400, share.PrimaryVolume);
            Assert.Equal(600, share.CrossStreetVolume);
            Assert.Equal(60, share.CrossTrafficPercent);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BuildSplitPressure_MixedAxisPrimaryDirectionsNeverOverlapCrossTraffic(bool hasSouthbound)
        {
            var profiles = new List<TimeOfDayProfileDto>
            {
                BuildProfile("Eastbound", "Eastbound", 480, 600),
                BuildProfile("Westbound", "Westbound", 480, 500),
                BuildProfile("Northbound", "Northbound", 480, 300)
            };
            if (hasSouthbound) profiles.Add(BuildProfile("Southbound", "Southbound", 480, 200));
            var result = CreateService().BuildSplitPressure(
                new TimeOfDayOptions { AllDayPrimaryDirections = new List<string> { "Eastbound", "Northbound" } },
                profiles, new List<TimeOfDayLocationAnalysisData>(), new List<DateOnly> { TestDate }, 15);

            Assert.Empty(result.PrimaryDirections.Intersect(result.CrossDirections));
            Assert.Equal(hasSouthbound ? new[] { "Southbound" } : Array.Empty<string>(), result.CrossDirections);
            var share = Assert.Single(result.CrossTrafficShare);
            Assert.Equal(900, share.PrimaryVolume);
            Assert.Equal(hasSouthbound ? 200 : 0, share.CrossStreetVolume);
            Assert.DoesNotContain("Westbound", result.CrossDirections);
        }

        [Fact]
        public void BuildSplitPressure_PreservesPeriodBoundariesAndLocationOrderForTiedPeaks()
        {
            var locations = new[] { "1002", "1001" }
                .Select(identifier => new TimeOfDayLocationAnalysisData
                {
                    Location = new Location { LocationIdentifier = identifier },
                    Observations = new[] { 480, 495, 600, 900, 1140 }
                        .SelectMany(minutes => new[] { "Eastbound", "Northbound" }
                            .Select(direction => BuildObservation(identifier, direction, minutes == 1140 ? 1000 : 25)
                                with { Minutes = minutes }))
                        .ToList()
                })
                .ToList();

            var result = CreateService().BuildSplitPressure(
                new TimeOfDayOptions { AllDayPrimaryDirections = new() { "Eastbound" } },
                BuildDirectionalProfiles(),
                locations,
                new[] { TestDate },
                15);

            Assert.Equal(
                new[] { ("AM", "1002", 480), ("AM", "1001", 480),
                    ("Midday", "1002", 600), ("Midday", "1001", 600),
                    ("PM", "1002", 900), ("PM", "1001", 900) },
                result.CrossTrafficLocations.Select(row => (row.Period, row.LocationIdentifier, row.Minutes)));
            Assert.All(result.CrossTrafficLocations, row =>
            {
                Assert.Equal(100, row.TotalVehiclesPerHour);
                Assert.Equal(50, row.PercentOfCrossTraffic);
            });
            Assert.Equal(
                new[] { ("AM", "1002", "08:00"), ("AM", "1001", "08:00"),
                    ("PM", "1002", "15:00"), ("PM", "1001", "15:00") },
                result.MovementPressures.Select(row => (row.Period, row.LocationIdentifier, row.PeakTime)));
            Assert.All(result.MovementPressures, row => Assert.Equal(200, row.Volume));
        }

        [Fact]
        public void BuildSplitPressure_InfersBothDirectionsOfPrimaryAxisWhenSelectionIsEmpty()
        {
            var result = CreateService().BuildSplitPressure(new TimeOfDayOptions(), new[]
            {
                BuildProfile("Eastbound", "Eastbound", 480, 600),
                BuildProfile("Westbound", "Westbound", 480, 600),
                BuildProfile("Northbound", "Northbound", 480, 200),
                BuildProfile("Southbound", "Southbound", 480, 200)
            }, Array.Empty<TimeOfDayLocationAnalysisData>(), new[] { TestDate }, 15);

            Assert.Equal(new[] { "Eastbound", "Westbound" }, result.PrimaryDirections);
            Assert.Equal(25, Assert.Single(result.CrossTrafficShare).CrossTrafficPercent);
        }

        [Fact]
        public void BuildSplitPressure_AppliesPeriodDirectionsToProfilesAndLocationRows()
        {
            var location = BuildLocation("1001", 150, 100);
            foreach (var minute in new[] { 600, 840, 1140 })
            {
                location.Observations.Add(BuildObservation("1001", "Eastbound", 150) with { Minutes = minute });
                location.Observations.Add(BuildObservation("1001", "Northbound", 100) with { Minutes = minute });
            }
            var result = CreateService().BuildSplitPressure(new TimeOfDayOptions
            {
                AllDayPrimaryDirections = new() { "Eastbound" },
                AmPrimaryDirections = new() { "Northbound" },
                PmPrimaryDirections = new() { "Northbound" }
            }, BuildDirectionalProfiles(), new[] { location }, new[] { TestDate }, 15);

            foreach (var minute in new[] { 480, 840 })
            {
                var point = result.CrossTrafficShare.Single(point => point.Minutes == minute);
                Assert.Equal(400, point.PrimaryVolume);
                Assert.Equal(600, point.CrossStreetVolume);
            }
            foreach (var minute in new[] { 600, 1140 })
            {
                Assert.Equal(600, result.CrossTrafficShare.Single(point => point.Minutes == minute).PrimaryVolume);
            }
            Assert.Equal(60, result.CrossTrafficLocations.Single(row => row.Period == "AM").PercentOfCrossTraffic);
            Assert.Equal(40, result.CrossTrafficLocations.Single(row => row.Period == "Midday").PercentOfCrossTraffic);
            Assert.Equal(60, result.CrossTrafficLocations.Single(row => row.Period == "PM").PercentOfCrossTraffic);
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
