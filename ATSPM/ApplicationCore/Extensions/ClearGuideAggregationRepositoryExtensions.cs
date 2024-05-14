using ATSPM.Application.Business.RouteSpeed;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    public static class ClearGuideRepositoryExtensions
    {

        public static List<RouteSpeed> GetRoutesSpeeds(
            this IClearguideAggregationRepository clearguideAggregationRepository,
        DateOnly startDate,
        DateOnly endDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string daysOfWeek,
        int violationThreshold,
        int sourceId,
        AnalysisPeriod analysisPeriod)
        {
            try
            {
                var startDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startTime.Hour, startTime.Minute, startTime.Second);
                var endDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, endTime.Hour, endTime.Minute, endTime.Second);
                int[] selectedDaysOfWeek = daysOfWeek.Split(',').Select(s => Convert.ToInt32(s.Trim())).ToArray();
                var query = clearguideAggregationRepository.GetAllAggregationsBetweenDates(startDateTime, endDateTime);
                switch (analysisPeriod)
                {
                    //case AnalysisPeriod.AllDay:
                    //    var startDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startTime.Hour, startTime.Minute, startTime.Second);
                    //    var endDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, endTime.Hour, endTime.Minute, endTime.Second);

                    //    query
                    //        .Where(record =>
                    //           record.Start.Date.ToDateTime(new TimeOnly(0, 0)) + record.BinStartTime.ToTimeSpan() >= startDateTime &&
                    //           record.Date.ToDateTime(new TimeOnly(0, 0)) + record.BinStartTime.ToTimeSpan() <= endDateTime);
                    //    break;
                    case AnalysisPeriod.PeekPeriod:
                        query
                            .Where(record => DateOnly.FromDateTime(record.Start) >= startDate &&
                                     DateOnly.FromDateTime(record.Start) <= endDate &&
                                     ((TimeOnly.FromDateTime(record.Start) >= new TimeOnly(6, 0) &&
                                     TimeOnly.FromDateTime(record.Start) <= new TimeOnly(9, 0))
                                     ||
                                     (TimeOnly.FromDateTime(record.Start) >= new TimeOnly(15, 0) &&
                                     TimeOnly.FromDateTime(record.Start) <= new TimeOnly(18, 0))
                                     ));
                        break;
                    default:
                        query
                            .Where(record => DateOnly.FromDateTime(record.Start) >= startDate &&
                                     DateOnly.FromDateTime(record.Start) <= endDate &&
                                     TimeOnly.FromDateTime(record.Start) >= startTime &&
                                     TimeOnly.FromDateTime(record.Start) <= endTime);
                        break;
                }
                var avgDailyResults = query.ToList().Where(s => selectedDaysOfWeek.Contains((int)s.Start.DayOfWeek)).ToList(); //.Where(speed => selectedDaysOfWeek.Contains(speed.Date.DayOfWeek.ToString())).ToList();

                var tempRoutes = Enumerable.Range(1, 6000).Select(x => x.ToString()).ToList(); //_dbContext.Routes.ToList();
                var routes = new Dictionary<string, int>();
                foreach (var route in tempRoutes)
                {
                    routes.Add(route, 45);
                }

                var results = avgDailyResults
                    .GroupBy(speed => speed.LocationIdentifier)
                    .Select(g =>
                    {
                        // Look into each hour violation find its value using the formula mentioned and then average that out
                        var speedLimit = routes[g.Key];
                        long summedFlows = 0;
                        var summedViolations = g.Sum(s =>
                        {
                            if (speedLimit <= 0)
                            {
                                return 0;
                            }
                            if (s.FifteenthSpeed.HasValue && HigherThanViolationThreshold(violationThreshold, (int)s.FifteenthSpeed, speedLimit))
                            {
                                if (s.Flow.HasValue)
                                {
                                    summedFlows += s.Flow.Value;
                                }
                                return s.Flow.HasValue ? (long)(s.Flow * .85) * s.Flow : null;
                            }
                            else if (HigherThanViolationThreshold(violationThreshold, s.Average, speedLimit))
                            {
                                if (s.Flow.HasValue)
                                {
                                    summedFlows += s.Flow.Value;
                                }
                                return s.Flow.HasValue ? (long)(s.Flow * .5) * s.Flow : null;
                            }
                            else if (s.EightyFifthSpeed.HasValue && HigherThanViolationThreshold(violationThreshold, (int)s.EightyFifthSpeed, speedLimit))
                            {
                                if (s.Flow.HasValue)
                                {
                                    summedFlows += s.Flow.Value;
                                }
                                return s.Flow.HasValue ? (long)(s.Flow * .15) * s.Flow : null;
                            }
                            else if (s.NinetyFifthSpeed.HasValue && HigherThanViolationThreshold(violationThreshold, (int)s.NinetyFifthSpeed, speedLimit))
                            {
                                if (s.Flow.HasValue)
                                {
                                    summedFlows += s.Flow.Value;
                                }
                                return s.Flow.HasValue ? (long)(s.Flow * .05) * s.Flow : null;
                            }
                            return 0;
                        });

                        return new RouteSpeed
                        {
                            RouteId = g.Key,
                            Name = g.Key, //route.Name,
                            Avg = (int)Math.Round(g.Average(s => s.Average)),
                            //AverageSpeedAboveSpeedLimit = (int)Math.Round(g.Average(s => (s.Average - route.SpeedLimit) / route.SpeedLimit)),
                            Percentilespd_15 = GetPercentileSpeed_15(g),
                            Percentilespd_85 = GetPercentileSpeed_85(g),
                            Percentilespd_95 = GetPercentileSpeed_95(g),
                            Flow = g.Sum(s => s.Flow),
                            //Percentilespd_99 = (int)Math.Round(g.Where(s => s.NinetyNinthSpeed.HasValue).Average(s => s.NinetyNinthSpeed.Value)),
                            //Violation = (int)Math.Round(g.Where(s => s.Violation.HasValue).Average(s => s.Violation.Value)),
                            EstimatedViolations = summedViolations > 0 ? Convert.ToInt32(Math.Round((double)summedViolations / summedFlows)) : null,
                            SpeedLimit = speedLimit, //route.SpeedLimit,
                            //Shape = routes.Where(s => s.Id == g.Key).FirstOrDefault().Shape
                        };
                    })
                    .ToList();

                updateRouteSpeeds(results);

                return results;
            }
            catch (Exception ex)
            {
                //logger.LogError(ex.Message, $"Error getting route speeds {_dbContext.Database.GetConnectionString()}");
                throw ex;
            }
        }

        private static void updateRouteSpeeds(List<RouteSpeed> results)
        {
            foreach (var speedRoute in results)
            {
                speedRoute.AverageSpeedAboveSpeedLimit = (int?)(speedRoute.SpeedLimit > 0 && speedRoute.Avg > speedRoute.SpeedLimit ? (int)Math.Round((((double)speedRoute.Avg - speedRoute.SpeedLimit) / (double)speedRoute.SpeedLimit) * 100) : null);

            }
        }

        private static int? GetPercentileSpeed_15(IGrouping<string, ClearGuideAggregation> g)
        {
            var speedList = g.Where<ClearGuideAggregation>(s => s.FifteenthSpeed.HasValue)
                                   .Select(s => s.FifteenthSpeed.Value)
                                   .ToList();
            return speedList.Count > 0 ? (int?)Math.Round(speedList.Average()) : null;
        }

        private static int? GetPercentileSpeed_85(IGrouping<string, ClearGuideAggregation> g)
        {
            var speedList = g.Where<ClearGuideAggregation>(s => s.EightyFifthSpeed.HasValue)
                                   .Select(s => s.EightyFifthSpeed.Value)
                                   .ToList();
            return speedList.Count > 0 ? (int?)Math.Round(speedList.Average()) : null;
        }

        private static int? GetPercentileSpeed_95(IGrouping<string, ClearGuideAggregation> g)
        {
            var speedList = g.Where<ClearGuideAggregation>(s => s.NinetyFifthSpeed.HasValue)
                                   .Select(s => s.NinetyFifthSpeed.Value)
                                   .ToList();
            return speedList.Count > 0 ? (int?)Math.Round(speedList.Average()) : null;
        }

        private static bool HigherThanViolationThreshold(int violationThreshold, int speed, int speedLimit)
        {
            var test = (speed - speedLimit) / (double)speedLimit * 100;
            return speed > speedLimit && (speed - speedLimit) / (double)speedLimit * 100 >= violationThreshold;
        }


    }
}
