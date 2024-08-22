using ATSPM.Application.Repositories.SpeedManagementRepositories;
using System.Collections.Generic;
using System.Linq;
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
                SegmentId = options.SegmentId
            };

            var sources = new List<int> { 1, 3 };
            var dayOfWeekValues = options.DaysOfWeek.Select(day => (int)day);

            // Convert the values to a comma-separated string
            string commaSeparatedDays = string.Join(",", dayOfWeekValues);

            foreach (var sourceId in sources)
            {
                var monthlyAverages = await this.hourlySpeedRepository.GetMonthlyAveragesAsync(options.SegmentId, options.StartDate, options.EndDate, commaSeparatedDays, sourceId);
                var dailyAverages = await this.hourlySpeedRepository.GetDailyAveragesAsync(options.SegmentId, options.StartDate, options.EndDate, commaSeparatedDays);

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
