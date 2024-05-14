using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.AggregationRepositories;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.RouteSpeed
{

    public enum AnalysisPeriod
    {
        AllDay,
        PeekPeriod,
        CustomHour
    }
    public class RouteSpeedService
    {
        private readonly IClearguideAggregationRepository routeSpeedRepository;

        public RouteSpeedService(IClearguideAggregationRepository crashSpeedRepository)
        {
            routeSpeedRepository = crashSpeedRepository;
        }

        public List<RouteSpeed> GetRouteSpeeds(
            DateOnly startDate,
            DateOnly endDate,
            TimeOnly startTime,
            TimeOnly endTime,
            string daysOfWeek,
            int violationThreshold,
            int sourceId,
            AnalysisPeriod analysisPeriod)

        {
            List<RouteSpeed> routeSpeeds = routeSpeedRepository.GetRoutesSpeeds(
                startDate,
                endDate,
                startTime,
                endTime,
                daysOfWeek,
                violationThreshold,
                sourceId,
                analysisPeriod);

            return routeSpeeds;
        }

        //public async Task<HistoricalDTO> GetHistoricalSpeeds(
        //    int routeId,
        //    DateOnly startDate,
        //    DateOnly endDate,
        //    TimeOnly startTime,
        //    TimeOnly endTime,
        //    string daysOfWeek,
        //    int percentile,
        //    AnalysisPeriod analysisPeriod)

        //{
        //    var routeSpeeds = new HistoricalDTO
        //    {
        //        RouteId = routeId
        //    };
        //    var sources = new List<int> { 1, 2, 3 };
        //    foreach (var soureceId in sources)
        //    {
        //        var result = await routeSpeedRepository.GetHistoricalSpeedsAsync(
        //            routeId,
        //            startDate,
        //            endDate,
        //            startTime,
        //            endTime,
        //            daysOfWeek,
        //            soureceId,
        //            percentile,
        //            analysisPeriod);
        //        if (result.Item1.Count > 0)
        //        {
        //            var monthlyAverages = result.Item1.Select(x => new MonthlyAverage
        //            {
        //                Month = x.Month,
        //                Year = x.Year,
        //                AverageSpeed = x.AverageSpeed
        //            }).OrderByDescending(a => a.Year).ThenByDescending(a => a.Month).ToList();
        //            routeSpeeds.MonthlyHistoricalRouteData.Add(new MonthlyHistoricalRouteData
        //            {
        //                SourceId = soureceId,
        //                MonthlyAverages = monthlyAverages
        //            });
        //        }
        //        else
        //        {
        //            routeSpeeds.MonthlyHistoricalRouteData.Add(new MonthlyHistoricalRouteData
        //            {
        //                SourceId = soureceId,
        //                MonthlyAverages = new List<MonthlyAverage>()
        //            });
        //        }

        //        if (result.Item2.Count > 0)
        //        {
        //            var dailyAverages = result.Item2.Select(x => new DailyAverage
        //            {
        //                Date = x.Date,
        //                AverageSpeed = x.AverageSpeed
        //            }).OrderByDescending(a => a.Date).ThenByDescending(a => a.Date).ToList();
        //            routeSpeeds.DailyHistoricalRouteData.Add(new DailyHistoricalRouteData
        //            {
        //                SourceId = soureceId,
        //                DailyAverages = dailyAverages
        //            });
        //        }
        //        else
        //        {
        //            routeSpeeds.DailyHistoricalRouteData.Add(new DailyHistoricalRouteData
        //            {
        //                SourceId = soureceId,
        //                DailyAverages = new List<DailyAverage>()
        //            });
        //        }


        //    }

        //    return routeSpeeds;
        //}
    }
}
