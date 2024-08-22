using ATSPM.Application.Business.RouteSpeed;
using ATSPM.Data.Models.SpeedManagement.Common;
using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
{
    public interface IHourlySpeedRepository : IAsyncRepository<HourlySpeed>
    {
        public Task AddHourlySpeedAsync(HourlySpeed hourlySpeed);
        public Task AddHourlySpeedsAsync(List<HourlySpeed> hourlySpeeds);
        public Task<List<MonthlyAverage>> GetMonthlyAveragesAsync(Guid segmentId, DateOnly startDate, DateOnly endDate, string daysOfWeek, int sourceId);
        public Task<List<DailyAverage>> GetDailyAveragesAsync(Guid segmentId, DateOnly startDate, DateOnly endDate, string daysOfWeek);
        public Task<List<RouteSpeed>> GetRoutesSpeeds(RouteSpeedOptions options);
        public Task<List<HourlySpeed>> GetHourlySpeedsForSegmentInSource(OptionsBase baseOptions, Guid segmentId);
        public Task<List<HourlySpeed>> GetWeeklySpeedsForSegmentInSource(OptionsBase baseOptions, Guid segmentId);
        Task<List<HourlySpeed>> GetHourlySpeedsForTimePeriod(Guid segmentId, DateTime startDate, DateTime endDate, DateTime startTime, DateTime endTime);
        #region ExtensionMethods



        #endregion

        #region Obsolete


        #endregion
    }
}