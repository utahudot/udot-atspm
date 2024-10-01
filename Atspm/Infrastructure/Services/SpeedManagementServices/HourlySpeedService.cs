using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices
{
    public enum AnalysisPeriod
    {
        AllDay,
        PeekPeriod,
        CustomHour
    }
    public class HourlySpeedService
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository segmentRepository;

        public HourlySpeedService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository segmentRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentRepository = segmentRepository;
        }

        public async Task<List<HourlySpeed>> GetHourlySpeedsForTimePeriod(Guid segmentId, DateTime startDate, DateTime endDate, DateTime startTime, DateTime endTime)
        {
            var hourlySpeeds = await hourlySpeedRepository.GetHourlySpeedsForTimePeriod(segmentId, startDate, endDate, startTime, endTime);
            return hourlySpeeds;
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

        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////

    }
}