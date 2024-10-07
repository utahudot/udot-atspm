using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface IMonthlyAggregationRepository : IAsyncRepository<MonthlyAggregation>
    {
        Task<List<MonthlyAggregationSimplified>> AllAggregationsOverTimePeriod(TimePeriodFilter timePeriod, MonthAggClassification dayType);
        Task<MonthlyAggregation> CheckExistanceAsync(MonthlyAggregation item);
        Task<List<RouteSpeed>> GetRoutesSpeeds(MonthlyAggregationOptions options);
        Task<List<RouteSpeed>> GetTopMonthlyAggregationsInCategory(MonthlyAggregationOptions options);
        Task<List<MonthlyAggregationSimplified>> LatestOfEachSegmentId(TimePeriodFilter timePeriod, MonthAggClassification monthAggClassification);
        Task<List<MonthlyAggregationSimplified>> MonthlyAggregationsForSegmentInTimePeriod(List<Guid> segmentIds, DateTime startTime, DateTime endTime, TimePeriodFilter timePeriod, MonthAggClassification monthAggClassification);
        Task RemoveKeyAsync(Guid? key);
        Task RemoveKeysAsync(List<Guid>? keys);
        Task RemoveBySegmentId(Guid segmentId);
        Task RemoveBySegmentIds(List<Guid> segmentIds);
        Task<List<MonthlyAggregationSimplified>> SelectMonthlyAggregationBySegment(Guid segmentId, TimePeriodFilter timePeriod, MonthAggClassification monthAggClassification);
        Task UpsertMonthlyAggregationAsync(MonthlyAggregation item);
    }
}
