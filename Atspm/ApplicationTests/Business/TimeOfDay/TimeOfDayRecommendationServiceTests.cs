#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay/TimeOfDayRecommendationServiceTests.cs
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
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.TimeOfDay;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Xunit;

namespace Utah.Udot.ATSPM.ApplicationTests.Business.TimeOfDay
{
    public class TimeOfDayRecommendationServiceTests
    {
        private static readonly DateOnly TestDate = new(2026, 1, 1);

        [Fact]
        public void BuildRecommendation_SelectsPythonStyleCommuteBoundaries()
        {
            var result = CreateService().BuildRecommendation(
                new TimeOfDayOptions(),
                BuildProfile(minutes =>
                {
                    if (minutes >= 360 && minutes <= 510)
                    {
                        return minutes == 450 ? 500 : 450;
                    }

                    if (minutes >= 900 && minutes <= 1080)
                    {
                        return minutes == 1020 ? 700 : 650;
                    }

                    return 100;
                }),
                new List<TimeOfDayProfileDto>(),
                TestDate);

            Assert.Equal(360, StartMinutes(FindPlan(result.RecommendedSchedule, "1")));
            Assert.Equal(495, EndMinutes(FindPlan(result.RecommendedSchedule, "1")));
            Assert.Equal(900, StartMinutes(FindPlan(result.RecommendedSchedule, "13")));
            Assert.Equal(1065, EndMinutes(FindPlan(result.RecommendedSchedule, "13")));
            Assert.Equal(1140, StartMinutes(result.RecommendedSchedule.Last(p => p.PlanNumber == "254")));
        }

        [Fact]
        public void BuildRecommendation_UsesFreeFallbackWhenEveningNeverDrops()
        {
            var result = CreateService().BuildRecommendation(
                new TimeOfDayOptions(),
                BuildProfile(minutes =>
                {
                    if (minutes >= 360 && minutes <= 510)
                    {
                        return minutes == 450 ? 500 : 450;
                    }

                    if (minutes >= 900 && minutes <= 1080)
                    {
                        return minutes == 1020 ? 700 : 650;
                    }

                    if (minutes >= 1140 && minutes <= 1410)
                    {
                        return 300;
                    }

                    return 100;
                }),
                new List<TimeOfDayProfileDto>(),
                TestDate);

            Assert.Equal(1410, StartMinutes(result.RecommendedSchedule.Last(p => p.PlanNumber == "254")));
        }

        [Fact]
        public void BuildRecommendation_DoesNotStartPmBeforeFourteenHundred()
        {
            var result = CreateService().BuildRecommendation(
                new TimeOfDayOptions(),
                BuildProfile(minutes =>
                {
                    if (minutes >= 360 && minutes <= 510)
                    {
                        return minutes == 450 ? 500 : 450;
                    }

                    if (minutes >= 780 && minutes <= 960)
                    {
                        return minutes == 855 ? 700 : 650;
                    }

                    return 100;
                }),
                new List<TimeOfDayProfileDto>(),
                TestDate);

            Assert.Equal(840, StartMinutes(FindPlan(result.RecommendedSchedule, "13")));
        }

        [Fact]
        public void BuildRecommendation_CapsAmEndAtConfiguredMaximum()
        {
            var result = CreateService().BuildRecommendation(
                new TimeOfDayOptions
                {
                    EntrySustainedBins = 1
                },
                BuildProfile(minutes =>
                {
                    if (minutes >= 360 && minutes <= 660)
                    {
                        return minutes == 450 ? 500 : 450;
                    }

                    if (minutes >= 900 && minutes <= 1080)
                    {
                        return minutes == 1020 ? 700 : 650;
                    }

                    return 100;
                }),
                new List<TimeOfDayProfileDto>(),
                TestDate);

            Assert.Equal(600, EndMinutes(FindPlan(result.RecommendedSchedule, "1")));
        }

        [Fact]
        public void BuildRecommendation_UsesConfiguredAmAndPmExitThresholds()
        {
            var highExitThresholdResult = CreateService().BuildRecommendation(
                new TimeOfDayOptions
                {
                    AmExitPctOfPeak = 0.8,
                    PmExitPctOfPeak = 0.8,
                    EntrySustainedBins = 1
                },
                BuildExitThresholdProfile(),
                new List<TimeOfDayProfileDto>(),
                TestDate);
            var lowExitThresholdResult = CreateService().BuildRecommendation(
                new TimeOfDayOptions
                {
                    AmExitPctOfPeak = 0.2,
                    PmExitPctOfPeak = 0.2,
                    EntrySustainedBins = 1
                },
                BuildExitThresholdProfile(),
                new List<TimeOfDayProfileDto>(),
                TestDate);

            Assert.True(
                EndMinutes(FindPlan(lowExitThresholdResult.RecommendedSchedule, "1")) >
                EndMinutes(FindPlan(highExitThresholdResult.RecommendedSchedule, "1")));
            Assert.True(
                EndMinutes(FindPlan(lowExitThresholdResult.RecommendedSchedule, "13")) >
                EndMinutes(FindPlan(highExitThresholdResult.RecommendedSchedule, "13")));
        }

        [Fact]
        public void BuildRecommendation_ReturnsUnavailableWhenProfileIsNotUsable()
        {
            var result = CreateService().BuildRecommendation(
                new TimeOfDayOptions(),
                BuildProfile(_ => 0),
                new List<TimeOfDayProfileDto>(),
                TestDate);

            Assert.Empty(result.RecommendedSchedule);
            Assert.Contains("no usable volume profile", result.SummaryText);
        }

        [Fact]
        public void BuildRecommendation_ReturnsUnavailableWhenExplicitPrimaryDirectionIsMissing()
        {
            var result = CreateService().BuildRecommendation(
                new TimeOfDayOptions
                {
                    AmPrimaryDirections = new List<string> { "Northbound" }
                },
                BuildProfile(_ => 500),
                new List<TimeOfDayProfileDto>
                {
                    BuildProfile(_ => 500, "Eastbound")
                },
                TestDate);

            Assert.Empty(result.RecommendedSchedule);
            Assert.Equal("tod-v2", result.AlgorithmVersion);
            Assert.Contains("AM: Northbound", result.SummaryText);
        }

        private static TimeOfDayRecommendationService CreateService()
        {
            return new TimeOfDayRecommendationService(new TimeOfDayProfileService());
        }

        private static TimeOfDayProfileDto BuildProfile(Func<int, double> volumeByMinute, string direction = "")
        {
            var points = Enumerable.Range(0, 96)
                .Select(i =>
                {
                    var minutes = i * 15;
                    var volume = volumeByMinute(minutes);
                    return new TimeOfDayProfilePointDto
                    {
                        TimeOfDay = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(minutes)).ToString("HH:mm"),
                        Minutes = minutes,
                        AverageVolume = volume,
                        SmoothedVolume = volume
                    };
                })
                .ToList();

            return new TimeOfDayProfileDto
            {
                Label = string.IsNullOrWhiteSpace(direction) ? "Corridor" : direction,
                Direction = direction,
                Points = points
            };
        }

        private static TimeOfDayProfileDto BuildExitThresholdProfile()
        {
            return BuildProfile(minutes => minutes switch
            {
                >= 360 and < 450 => 800,
                450 => 1000,
                465 => 800,
                480 => 600,
                495 => 400,
                510 => 200,
                >= 900 and < 1020 => 800,
                1020 => 1000,
                1035 => 800,
                1050 => 600,
                1065 => 400,
                1080 => 200,
                _ => 100
            });
        }

        private static Plan FindPlan(IReadOnlyList<Plan> schedule, string planNumber)
        {
            return schedule.Single(p => p.PlanNumber == planNumber);
        }

        private static int StartMinutes(Plan plan)
        {
            return MinutesFromDate(plan.Start);
        }

        private static int EndMinutes(Plan plan)
        {
            return MinutesFromDate(plan.End);
        }

        private static int MinutesFromDate(DateTime value)
        {
            return (int)(value - TestDate.ToDateTime(TimeOnly.MinValue)).TotalMinutes;
        }
    }
}
