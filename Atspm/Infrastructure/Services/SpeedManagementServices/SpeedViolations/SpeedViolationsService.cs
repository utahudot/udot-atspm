using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverDistance;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedViolations
{
    public class SpeedViolationsService : ReportServiceBase<SpeedViolationsOptions, List<SpeedViolationsDto>>
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository segmentRepository;

        public SpeedViolationsService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository segmentRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentRepository = segmentRepository;
        }

        public override async Task<List<SpeedViolationsDto>> ExecuteAsync(SpeedViolationsOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var filteredHours = await hourlySpeedRepository.GetHourlySpeedsWithFiltering(parameter.SegmentIds, parameter.StartDate, parameter.EndDate, parameter.StartTime, parameter.EndTime, parameter.DayOfWeek, parameter.SpecificDays);

            var groupedBySegmentId = filteredHours.GroupBy(h => h.SegmentId).ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<SpeedViolationsDto>();
            foreach (var segmentInfo in groupedBySegmentId)
            {
                var segmentId = segmentInfo.Key;
                var segment = await segmentRepository.LookupAsync(segmentId);
                var hourlySpeeds = segmentInfo.Value;

                List<DailySpeedViolationsDto> aggregatedByDay = hourlySpeeds
                    .GroupBy(h => h.Date.Date)
                    .Select(g => new DailySpeedViolationsDto
                    {
                        Date = g.Key,
                        DailyFlow = g.Sum(h => h.Flow ?? 0), // Sum violations, ignoring nulls
                        DailyViolationsCount = g.Sum(h => h.Violation ?? 0), // Sum violations, ignoring nulls
                        DailyExtremeViolationsCount = g.Sum(h => h.ExtremeViolation ?? 0), // Sum extreme violations
                        DailyPercentViolations = g.Sum(h => h.Flow ?? 0) != 0 ? (double)g.Sum(h => h.Violation ?? 0) / g.Sum(h => h.Flow ?? 0) : 0,
                        DailyPercentExtremeViolations = g.Sum(h => h.Flow ?? 0) != 0 ? (double)g.Sum(h => h.ExtremeViolation ?? 0) / g.Sum(h => h.Flow ?? 0) : 0
                    })
                    .ToList();

                var segmentSpeedViolations = new SpeedViolationsDto
                {
                    SegmentId = segmentId,
                    SegmentName = segment.Name,
                    TotalFlow = aggregatedByDay.Sum(f => f.DailyFlow),
                    TotalViolationsCount = aggregatedByDay.Sum(f => f.DailyViolationsCount),
                    TotalExtremeViolationsCount = aggregatedByDay.Sum(f => f.DailyExtremeViolationsCount),
                    PercentViolations = aggregatedByDay.Sum(h => h.DailyFlow) != 0 ? (double)aggregatedByDay.Sum(h => h.DailyViolationsCount) / aggregatedByDay.Sum(h => h.DailyFlow) : 0,
                    PercentExtremeViolations = aggregatedByDay.Sum(h => h.DailyFlow) != 0 ? (double)aggregatedByDay.Sum(h => h.DailyExtremeViolationsCount) / aggregatedByDay.Sum(h => h.DailyFlow) : 0,
                    SpeedLimit = segment.SpeedLimit,
                    dailySpeedViolationsDto = aggregatedByDay
                };

                result.Add(segmentSpeedViolations);
            }
            return result;
        }

    }
}
