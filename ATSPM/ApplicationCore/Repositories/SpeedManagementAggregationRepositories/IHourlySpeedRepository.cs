using ATSPM.Application.Business.RouteSpeed;
using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementAggregationRepositories
{
    public interface IHourlySpeedRepository : IAsyncRepository<HourlySpeed>
    {
        public Task AddHourlySpeedAsync(HourlySpeed hourlySpeed);
        public Task AddHourlySpeedsAsync(List<HourlySpeed> hourlySpeeds);
        public Task<List<MonthlyAverage>> GetMonthlyAveragesAsync(int routeId, DateOnly startDate, DateOnly endDate, string daysOfWeek, int sourceId);
        public Task<List<DailyAverage>> GetDailyAveragesAsync(int routeId, DateOnly startDate, DateOnly endDate, string daysOfWeek, int sourceId);
        public Task<List<RouteSpeed>> GetRoutesSpeeds(RouteSpeedOptions options);
        #region ExtensionMethods



        #endregion

        #region Obsolete


        #endregion
    }
}
