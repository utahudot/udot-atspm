using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagementAggregation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private int numRoutes = 14000;

        public RouteSpeedService(IHourlySpeedRepository hourlySpeedRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
        }

        public async Task<List<RouteSpeed>> GetRouteSpeedsAsync(RouteSpeedOptions options)

        {
            List<RouteSpeed> routeSpeeds = await hourlySpeedRepository.GetRoutesSpeeds(options);

            return routeSpeeds;
        }

        public async Task<HistoricalDTO> GetHistoricalSpeeds(HistoricalSpeedOptions options)
        {
            var routeSpeeds = new HistoricalDTO
            {
                RouteId = options.RouteId
            };

            var sources = new List<int> { 1, 3 };

            foreach (var sourceId in sources)
            {
                var monthlyAverages = await this.hourlySpeedRepository.GetMonthlyAveragesAsync(options.RouteId, options.StartDate, options.EndDate, options.DaysOfWeek.ToString(), sourceId);
                var dailyAverages = await this.hourlySpeedRepository.GetDailyAveragesAsync(options.RouteId, options.StartDate, options.EndDate, options.DaysOfWeek.ToString());

                routeSpeeds.MonthlyHistoricalRouteData.Add(new MonthlyHistoricalRouteData
                {
                    SourceId = sourceId,
                    MonthlyAverages = monthlyAverages
                });

                routeSpeeds.DailyHistoricalRouteData.Add(new DailyHistoricalRouteData
                {
                    SourceId = sourceId,
                    DailyAverages = dailyAverages
                });
            }

            return routeSpeeds;
        }
    }
}
