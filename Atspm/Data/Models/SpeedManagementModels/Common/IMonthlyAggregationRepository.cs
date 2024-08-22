using ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.SpeedManagementRepositories
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
