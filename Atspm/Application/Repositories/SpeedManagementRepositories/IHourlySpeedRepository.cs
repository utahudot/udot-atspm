using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface IHourlySpeedRepository : IAsyncRepository<HourlySpeed>
    {
        public Task AddHourlySpeedAsync(HourlySpeed hourlySpeed);
        public Task AddHourlySpeedsAsync(List<HourlySpeed> hourlySpeeds);
        public Task<List<MonthlyAverage>> GetMonthlyAveragesAsync(Guid segmentId, DateOnly startDate, DateOnly endDate, string daysOfWeek);
        public Task<List<DailyAverage>> GetDailyAveragesAsync(Guid segmentId, DateOnly startDate, DateOnly endDate, string daysOfWeek);
        public Task<List<RouteSpeed>> GetRoutesSpeeds(RouteSpeedOptions options);
        public Task<List<HourlySpeed>> GetHourlySpeedsForSegmentInSource(OptionsBase baseOptions, Guid segmentId);
        public Task<List<HourlySpeed>> GetWeeklySpeedsForSegmentInSource(OptionsBase baseOptions, Guid segmentId);
        Task<List<HourlySpeed>> GetHourlySpeedsForTimePeriod(Guid segmentId, DateTime startDate, DateTime endDate, DateTime startTime, DateTime endTime);
        Task<List<HourlySpeed>> HourlyAggregationsForSegmentInTimePeriod(List<Guid> segmentIds, DateTime startTime, DateTime endTime);
        Task<List<HourlySpeed>> GetHourlySpeedsWithFiltering(List<Guid> segmentIds, DateTime startDate, DateTime endDate, DateTime? startTime, DateTime? endTime, int? dayOfWeek, List<DateTime> specificDays);
        public Task DeleteBySegment(Guid segmentId);
        public Task DeleteBySegments(List<Guid> segments);
        #region ExtensionMethods



        #endregion

        #region Obsolete


        #endregion
    }
}