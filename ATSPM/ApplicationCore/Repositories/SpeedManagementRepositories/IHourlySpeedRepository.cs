using ATSPM.Application.Business.RouteSpeed;
using ATSPM.Data.Models.SpeedManagement.CongestionTracking;
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
        public Task<List<MonthlyAverage>> GetMonthlyAveragesAsync(int routeId, DateOnly startDate, DateOnly endDate, string daysOfWeek, int sourceId);
        public Task<List<DailyAverage>> GetDailyAveragesAsync(int routeId, DateOnly startDate, DateOnly endDate, string daysOfWeek);
        public Task<List<RouteSpeed>> GetRoutesSpeeds(RouteSpeedOptions options);
        public Task<List<HourlySpeed>> GetHourlySpeeds(CongestionTrackingOptions options);
        #region ExtensionMethods



        #endregion

        #region Obsolete


        #endregion
    }
}
