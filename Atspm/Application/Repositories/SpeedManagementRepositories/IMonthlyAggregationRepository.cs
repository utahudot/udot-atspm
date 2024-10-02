using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface IMonthlyAggregationRepository : IAsyncRepository<MonthlyAggregation>
    {
        Task<List<MonthlyAggregationSimplified>> AllAggregationsOverTimePeriod(TimePeriodFilter timePeriod, MonthAggClassification dayType);
        Task<MonthlyAggregation> CheckExistanceAsync(MonthlyAggregation item);
        Task<List<MonthlyAggregationSimplified>> GetTopMonthlyAggregationsInCategory(MonthlyAggregationOptions options);
        Task<List<MonthlyAggregationSimplified>> LatestOfEachSegmentId(TimePeriodFilter timePeriod, MonthAggClassification monthAggClassification);
        Task<List<MonthlyAggregationSimplified>> MonthlyAggregationsForSegmentInTimePeriod(List<Guid> segmentIds, DateTime startTime, DateTime endTime, TimePeriodFilter timePeriod, MonthAggClassification monthAggClassification);
        Task RemoveKeyAsync(Guid? key);
        Task<List<MonthlyAggregationSimplified>> SelectMonthlyAggregationBySegment(Guid segmentId, TimePeriodFilter timePeriod, MonthAggClassification monthAggClassification);
        Task UpsertMonthlyAggregationAsync(MonthlyAggregation item);
    }
}
