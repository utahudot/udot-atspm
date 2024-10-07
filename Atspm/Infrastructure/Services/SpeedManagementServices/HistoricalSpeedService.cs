using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices
{
    public class HistoricalSpeedService
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository segmentRepository;
        private readonly MonthlyAggregationService monthlyAggregationService;

        public HistoricalSpeedService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository segmentRepository, MonthlyAggregationService monthlyAggregationService)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentRepository = segmentRepository;
            this.monthlyAggregationService = monthlyAggregationService;
        }

        public async Task<List<DailyHistoricalRouteData>> GetDailyHistoricalData(HistoricalSpeedOptions options)
        {
            var dailySpeeds = new List<DailyHistoricalRouteData>();

            var sources = new List<int> { 1, 2, 3 };
            var dayOfWeekValues = options.DaysOfWeek.Select(day => (int)day);

            // Convert the values to a comma-separated string
            string commaSeparatedDays = string.Join(",", dayOfWeekValues);
            var dailyAveragesForAllSources = await hourlySpeedRepository.GetDailyAveragesAsync(options.SegmentId, options.StartDate, options.EndDate, commaSeparatedDays);
            foreach (var sourceId in sources)
            {
                dailySpeeds.Add(new DailyHistoricalRouteData
                {
                    SourceId = sourceId,
                    DailyAverages = dailyAveragesForAllSources
                    .Where(da => da.SourceId == sourceId)
                    .OrderBy(da => da.Date)
                    .ToList()
                });
            }

            return dailySpeeds;
        }

        public async Task<List<MonthlyHistoricalRouteData>> GetMonthlyHistoricalData(HistoricalSpeedOptions options)
        {
            var MonthlySpeeds = new List<MonthlyHistoricalRouteData>();

            var sources = new List<int> { 1, 2, 3 };
            var dayOfWeekValues = options.DaysOfWeek.Select(day => (int)day);

            // Convert the values to a comma-separated string
            string commaSeparatedDays = string.Join(",", dayOfWeekValues);
            var monthlyAveragesForAllSources = await monthlyAggregationService.ListMonthlyAggregationsForSegment(options.SegmentId, TimePeriodFilter.AllDay, MonthAggClassification.Total);
            foreach (var sourceId in sources)
            {
                MonthlySpeeds.Add(new MonthlyHistoricalRouteData
                {
                    SourceId = sourceId,
                    MonthlyAverages = monthlyAveragesForAllSources
                    .Where(ma => ma.SourceId == sourceId)
                    .OrderBy(ma => ma.BinStartTime)
                    .Select(ma => new MonthlyAverage
                    {
                        Month = ma.BinStartTime,
                        Average = ma.AverageSpeed.Value,
                        EightyFifthSpeed = ma.AverageEightyFifthSpeed.Value,
                        Violation = ma.Violations.Value,
                        ExtremeViolation = ma.ExtremeViolations.Value,
                        Flow = ma.Flow.Value,
                        MaxSpeed = ma.MaxSpeed.Value,
                        MinSpeed = ma.MinSpeed.Value,
                        SourceId = sourceId,
                    })
                    .ToList()
                });
            }

            return MonthlySpeeds;
        }
    }
}
