#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay/TimeOfDayServiceProfileTests.cs
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
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Xunit;

namespace Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay
{
    public class TimeOfDayServiceProfileTests
    {
        private static readonly DateOnly TestDate = new(2026, 1, 1);

        [Fact]
        public void GetChartData_UsesMedianRepresentativeProfilesAcrossLocations()
        {
            var profileService = new TimeOfDayProfileService();
            var observationsByLocation = new Dictionary<string, List<TimeOfDayVolumeObservation>>
            {
                ["1001"] = new() { BuildObservation("1001", "Eastbound", 25) },
                ["1002"] = new() { BuildObservation("1002", "Eastbound", 75) },
                ["1003"] = new() { BuildObservation("1003", "Eastbound", 225) }
            };
            var service = new TimeOfDayService(
                new TimeOfDayLocationService(new StubObservationService(observationsByLocation), profileService),
                profileService,
                new StubRecommendationService(),
                new TimeOfDayPlanScheduleService(new PlanService()),
                new TimeOfDayPlanProfileService(),
                new TimeOfDaySplitPressureService(profileService));

            var result = service.GetChartData(
                new TimeOfDayOptions(),
                observationsByLocation.Keys.ToList(),
                new List<DateOnly> { TestDate },
                observationsByLocation.Keys
                    .Select(id => new TimeOfDayLocationReportData
                    {
                        Location = new Location { LocationIdentifier = id },
                        LocationDescription = id
                    })
                    .ToList(),
                new List<TimeOfDayWarningDto>());

            var corridorPoint = result.PlanProfile.CorridorProfile.Points.Single(p => p.Minutes == 480);
            var eastboundProfile = Assert.Single(result.PlanProfile.DirectionalProfiles);
            var eastboundPoint = eastboundProfile.Points.Single(p => p.Minutes == 480);

            Assert.Equal(300, corridorPoint.AverageVolume);
            Assert.Equal(300, eastboundPoint.AverageVolume);
            Assert.All(result.Locations, location =>
            {
                Assert.Equal("Complete", location.DataQualityFlag);
                Assert.Equal(new[] { TestDate }, location.DatesWithData);
                Assert.Empty(location.MissingDates);
            });
        }

        [Fact]
        public void GetChartData_ReturnsCompletePartialAndNoDataLocationRows()
        {
            var secondDate = TestDate.AddDays(1);
            var observationsByLocation = new Dictionary<string, List<TimeOfDayVolumeObservation>>
            {
                ["1001"] = new()
                {
                    BuildObservation("1001", "Eastbound", 25, TestDate),
                    BuildObservation("1001", "Eastbound", 25, secondDate)
                },
                ["1002"] = new() { BuildObservation("1002", "Eastbound", 25, TestDate) },
                ["1003"] = new()
            };
            var reportData = observationsByLocation.Keys
                .Select(id => new TimeOfDayLocationReportData
                {
                    Location = new Location { LocationIdentifier = id },
                    LocationDescription = id
                })
                .ToList();
            reportData.Single(data => data.Location.LocationIdentifier == "1003").PlanEventsByDate.Add(TestDate, new[] {
                TimingPlan(TestDate.ToDateTime(TimeOnly.MinValue), 7, "1003") });
            var warnings = new List<TimeOfDayWarningDto>();

            var result = CreateService(observationsByLocation).GetChartData(
                new TimeOfDayOptions(),
                observationsByLocation.Keys.ToList(),
                new List<DateOnly> { TestDate, secondDate },
                reportData,
                warnings);

            var complete = result.Locations.Single(location => location.LocationIdentifier == "1001");
            var partial = result.Locations.Single(location => location.LocationIdentifier == "1002");
            var noData = result.Locations.Single(location => location.LocationIdentifier == "1003");

            Assert.Equal("Complete", complete.DataQualityFlag);
            Assert.Equal("Partial", partial.DataQualityFlag);
            Assert.Equal(new[] { secondDate }, partial.MissingDates);
            Assert.Contains(secondDate.ToString("yyyy-MM-dd"), partial.Summary.Notes);
            Assert.Equal("NoData", noData.DataQualityFlag);
            Assert.Empty(noData.DatesWithData);
            Assert.Equal(new[] { TestDate, secondDate }, noData.MissingDates);
            Assert.Contains("No usable volume observations", noData.Summary.Notes);
            Assert.Contains(noData.CurrentPlanSchedule, plan => plan.PlanNumber == "7");
            Assert.Contains(result.Warnings, warning => warning.Code == "PartialLocationData" && warning.LocationIdentifier == "1002");
            Assert.Contains(result.Warnings, warning => warning.Code == "NoLocationVolumeData" && warning.LocationIdentifier == "1003");
        }

        [Fact]
        public void GetChartData_ReturnsNoDataRowsWhenCorridorHasNoUsableVolume()
        {
            var observationsByLocation = new Dictionary<string, List<TimeOfDayVolumeObservation>>
            {
                ["1001"] = new()
            };
            var reportData = new TimeOfDayLocationReportData
            {
                Location = new Location { LocationIdentifier = "1001" },
                LocationDescription = "1001"
            };
            reportData.PlanEventsByDate.Add(TestDate, new[] {
                TimingPlan(TestDate.ToDateTime(TimeOnly.MinValue), 7, "1001") });

            var result = CreateService(observationsByLocation).GetChartData(
                new TimeOfDayOptions(),
                new List<string> { "1001" },
                new List<DateOnly> { TestDate },
                new List<TimeOfDayLocationReportData> { reportData },
                new List<TimeOfDayWarningDto>());

            var location = Assert.Single(result.Locations);
            Assert.Equal("NoData", location.DataQualityFlag);
            Assert.Contains(location.CurrentPlanSchedule, plan => plan.PlanNumber == "7");
            Assert.Contains(result.Warnings, warning => warning.Code == "NoUsableVolumeData");
        }

        [Fact]
        public void GetChartData_WarnsWhenRequestedPrimaryDirectionIsUnavailable()
        {
            var observationsByLocation = new Dictionary<string, List<TimeOfDayVolumeObservation>>
            {
                ["1001"] = new() { BuildObservation("1001", "Eastbound", 25) }
            };

            var result = CreateService(observationsByLocation).GetChartData(
                new TimeOfDayOptions { AmPrimaryDirections = new List<string> { "Northbound" } },
                new List<string> { "1001" },
                new List<DateOnly> { TestDate },
                new List<TimeOfDayLocationReportData>
                {
                    new()
                    {
                        Location = new Location { LocationIdentifier = "1001" },
                        LocationDescription = "1001"
                    }
                },
                new List<TimeOfDayWarningDto>());

            Assert.Contains(result.Warnings, warning =>
                warning.Code == "PrimaryDirectionDataUnavailable" &&
                warning.Message.Contains("AM") &&
                warning.Message.Contains("Northbound"));
        }

        [Fact]
        public void GetChartData_PopulatesLocationCrossTrafficReview()
        {
            var observations = new Dictionary<string, List<TimeOfDayVolumeObservation>>
            {
                ["1001"] = new()
                {
                    BuildObservation("1001", "Eastbound", 75),
                    BuildObservation("1001", "Northbound", 25)
                }
            };
            var result = CreateService(observations).GetChartData(
                new TimeOfDayOptions { AllDayPrimaryDirections = new() { "Eastbound" } },
                new[] { "1001" }, new[] { TestDate },
                new[] { new TimeOfDayLocationReportData { Location = new Location { LocationIdentifier = "1001" } } },
                new List<TimeOfDayWarningDto>());

            Assert.Contains("25%", Assert.Single(result.Locations).Summary.CrossTrafficReview);
        }

        private static TimeOfDayService CreateService(
            IReadOnlyDictionary<string, List<TimeOfDayVolumeObservation>> observationsByLocation)
        {
            var profileService = new TimeOfDayProfileService();
            return new TimeOfDayService(
                new TimeOfDayLocationService(new StubObservationService(observationsByLocation), profileService),
                profileService,
                new StubRecommendationService(),
                new TimeOfDayPlanScheduleService(new PlanService()),
                new TimeOfDayPlanProfileService(),
                new TimeOfDaySplitPressureService(profileService));
        }

        private static TimeOfDayVolumeObservation BuildObservation(
            string locationIdentifier,
            string direction,
            double count,
            DateOnly? localDate = null)
        {
            return new TimeOfDayVolumeObservation(
                locationIdentifier,
                locationIdentifier,
                localDate ?? TestDate,
                480,
                direction,
                "Thru",
                "Thru",
                count);
        }

        private static IndianaEvent TimingPlan(DateTime start, short planNumber, string locationIdentifier)
        {
            return new IndianaEvent
            {
                LocationIdentifier = locationIdentifier,
                EventCode = 131,
                EventParam = planNumber,
                Timestamp = start
            };
        }

        private class StubObservationService : ITimeOfDayObservationService
        {
            private readonly IReadOnlyDictionary<string, List<TimeOfDayVolumeObservation>> observationsByLocation;

            public StubObservationService(
                IReadOnlyDictionary<string, List<TimeOfDayVolumeObservation>> observationsByLocation)
            {
                this.observationsByLocation = observationsByLocation;
            }

            public TimeOfDayObservationBuildResult BuildIndianaEventObservations(
                Location location,
                string locationDescription,
                IReadOnlyList<DateOnly> selectedDates,
                int binSizeMinutes,
                IReadOnlyList<IndianaEvent> indianaEvents)
            {
                return new TimeOfDayObservationBuildResult(
                    observationsByLocation[location.LocationIdentifier],
                    true);
            }

            public TimeOfDayObservationBuildResult BuildAggregatedObservations(
                Location location,
                string locationDescription,
                IReadOnlyList<DateOnly> selectedDates,
                int binSizeMinutes,
                IReadOnlyList<DetectorEventCountAggregation> aggregations)
            {
                return BuildIndianaEventObservations(
                    location,
                    locationDescription,
                    selectedDates,
                    binSizeMinutes,
                    new List<IndianaEvent>());
            }
        }

        private class StubRecommendationService : ITimeOfDayRecommendationService
        {
            public TimeOfDayRecommendationDto BuildRecommendation(
                TimeOfDayOptions options,
                TimeOfDayProfileDto corridorProfile,
                IReadOnlyList<TimeOfDayProfileDto> directionalProfiles,
                DateOnly representativeDate)
            {
                return new TimeOfDayRecommendationDto();
            }
        }

    }
}
