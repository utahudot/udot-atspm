using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.SpeedManagementRepositories
{
    public interface IMonthlyAggregationRepository : IAsyncRepository<MonthlyAggregation>
    {
        Task<List<MonthlyAggregation>> AllAggregationsOverTimePeriod();
        Task<MonthlyAggregation> CheckExistanceAsync(MonthlyAggregation item);
        Task<List<MonthlyAggregation>> MonthlyAggregationsForSegmentInTimePeriod(List<Guid> segmentId, DateTime startTime, DateTime endTime);
        Task<List<MonthlyAggregation>> SelectBinStartTimesInRange(DateTime startTime, DateTime endTime, MonthlyAggregation monthlyAggregation);
        Task<List<MonthlyAggregation>> SelectBinStartTimesInRangeFromSource(DateTime startTime, DateTime endTime, MonthlyAggregation monthlyAggregation);
        Task<MonthlyAggregation> SelectByBinTimeSegment(DateTime binStartTime, MonthlyAggregation monthlyAggregation);
        Task<MonthlyAggregation> SelectByBinTimeSegmentAndSource(DateTime binStartTime, MonthlyAggregation monthlyAggregation);
        Task<List<MonthlyAggregation>> SelectMonthlyAggregationBySegment(Guid SegmentId);
        Task UpsertMonthlyAggregationAsync(MonthlyAggregation item);
    }
}
