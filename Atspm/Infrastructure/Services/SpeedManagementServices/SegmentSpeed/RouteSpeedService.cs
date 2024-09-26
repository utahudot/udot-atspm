using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SegmentSpeed
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

            var sources = new List<int> { 1, 2, 3 };
            var dayOfWeekValues = options.DaysOfWeek.Select(day => (int)day);

            // Convert the values to a comma-separated string
            string commaSeparatedDays = string.Join(",", dayOfWeekValues);
            var dailyAveragesForAllSources = await hourlySpeedRepository.GetDailyAveragesAsync(options.SegmentId, options.StartDate, options.EndDate, commaSeparatedDays);
            var monthlyAveragesForAllSources = await hourlySpeedRepository.GetMonthlyAveragesAsync(options.SegmentId, options.StartDate, options.EndDate, commaSeparatedDays);
            foreach (var sourceId in sources)
            {


                routeSpeeds.MonthlyHistoricalRouteData.Add(new MonthlyHistoricalRouteData
                {
                    SourceId = sourceId,
                    MonthlyAverages = monthlyAveragesForAllSources
                    .Where(ma => ma.SourceId == sourceId)
                    .OrderBy(ma => ma.Month)
                    .ToList()
                });

                routeSpeeds.DailyHistoricalRouteData.Add(new DailyHistoricalRouteData
                {
                    SourceId = sourceId,
                    DailyAverages = dailyAveragesForAllSources
                    .Where(da => da.SourceId == sourceId)
                    .OrderBy(da => da.Date)
                    .ToList()
                });
            }

            return routeSpeeds;
        }
    }
}