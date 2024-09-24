using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.EffectivenessOfStrategies;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.EffectivenessOrStrategies
{
    public class EffectivenessOfStrategiesService : ReportServiceBase<EffectivenessOfStrategiesOptions, List<EffectivenessOfStrategiesDto>>
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository segmentRepository;

        public EffectivenessOfStrategiesService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository segmentRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentRepository = segmentRepository;
        }

        public override async Task<List<EffectivenessOfStrategiesDto>> ExecuteAsync(EffectivenessOfStrategiesOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var implementationDate = parameter.StrategyImplementedDate;
            var startDate = implementationDate.AddDays(-42);
            DateTime dateSixWeeksAfter = implementationDate.AddDays(42);
            DateTime endDate = (dateSixWeeksAfter > DateTime.Now.AddDays(-1)) ? DateTime.Now.AddDays(-1) : dateSixWeeksAfter;

            var filteredHours = await hourlySpeedRepository.GetHourlySpeedsWithFiltering(parameter.SegmentIds, startDate, endDate, parameter.StartTime, parameter.EndTime, null, null);

            var groupedBySegmentId = filteredHours.GroupBy(h => h.SegmentId).ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<EffectivenessOfStrategiesDto>();
            foreach (var segmentInfo in groupedBySegmentId)
            {
                var segmentId = segmentInfo.Key;
                var segment = await segmentRepository.LookupAsync(segmentId);
                var hourlySpeeds = segmentInfo.Value;

                var startDateChunking = hourlySpeeds.Min(h => h.Date).Date; // Find the earliest date

                var aggregatedBy7Days = hourlySpeeds
                    .GroupBy(h => (h.Date.Date - startDateChunking).Days / 7) // Group into 7-day chunks
                    .Select(g => new TimeSegmentEffectiveness
                    {
                        StartDate = startDateChunking.AddDays(g.Key * 7),
                        EndDate = startDateChunking.AddDays((g.Key * 7) + 6), // Start of the 7-day chunk
                        Flow = (g.Sum(h => h.Flow)) ?? 0,
                        MaxSpeed = g.Max(h => h.MaxSpeed) ?? 0,
                        MinSpeed = g.Min(h => h.MinSpeed) ?? 0,
                        AverageSpeed = ((double)(g.Sum(h => h.Average)) / (g.Sum(h => h.Flow))) ?? 0,
                        AverageEightyFifthSpeed = ((double)(g.Sum(h => h.EightyFifthSpeed)) / (g.Sum(h => h.Flow))) ?? 0,
                        Variablitiy = ((double)g.Max(h => h.MaxSpeed ?? 0)) - ((double)g.Min(h => h.MinSpeed ?? 0)),
                        PercentViolations = (g.Sum(h => h.Flow) != 0 ? (double)g.Sum(h => h.Violation) / g.Sum(h => h.Flow) ?? 0 : 0) * 100,
                        PercentExtremeViolations = (g.Sum(h => h.Flow) != 0 ? (double)g.Sum(h => h.ExtremeViolation) / g.Sum(h => h.Flow) ?? 0 : 0) * 100,
                    })
                    .ToList();

                var aggregatedBefore = hourlySpeeds
                    .Where(h => h.Date.Date < implementationDate)
                    .GroupBy(h => h.SegmentId) // Filter for dates before the specific date
                    .Select(g => new TimeSegmentEffectiveness
                    {
                        StartDate = hourlySpeeds.Min(h => h.Date), // Earliest date in the filtered data
                        EndDate = implementationDate.AddDays(-1), // End the day before the specific date
                        Flow = g.Sum(h => h.Flow) ?? 0,
                        MaxSpeed = g.Max(h => h.MaxSpeed) ?? 0,
                        MinSpeed = g.Min(h => h.MinSpeed) ?? 0,
                        AverageSpeed = g.Sum(h => h.Flow) != 0 ? g.Sum(h => h.Average * (h.Flow ?? 0)) / g.Sum(h => h.Flow) ?? 0 : 0,
                        AverageEightyFifthSpeed = g.Sum(h => h.Flow) != 0 ? g.Sum(h => h.EightyFifthSpeed ?? 0) / g.Sum(h => h.Flow) ?? 0 : 0,
                        Variablitiy = ((double)g.Max(h => h.MaxSpeed ?? 0)) - ((double)g.Min(h => h.MinSpeed ?? 0)),
                        PercentViolations = (g.Sum(h => h.Flow) != 0 ? (double)(g.Sum(h => h.Violation ?? 0)) / g.Sum(h => h.Flow) ?? 0 : 0) * 100,
                        PercentExtremeViolations = (g.Sum(h => h.Flow) != 0 ? (double)(g.Sum(h => h.ExtremeViolation ?? 0)) / g.Sum(h => h.Flow) ?? 0 : 0) * 100,
                    })
                    .FirstOrDefault();

                var aggregatedAfter = hourlySpeeds
                    .Where(h => h.Date.Date >= implementationDate) // Filter for dates on or after the specific date
                    .GroupBy(h => h.SegmentId) // Filter for dates before the specific date
                    .Select(g => new TimeSegmentEffectiveness
                    {
                        StartDate = implementationDate, // Earliest date in the filtered data
                        EndDate = hourlySpeeds.Max(h => h.Date), // End the day before the specific date
                        Flow = g.Sum(h => h.Flow) ?? 0,
                        MaxSpeed = g.Max(h => h.MaxSpeed) ?? 0,
                        MinSpeed = g.Min(h => h.MinSpeed) ?? 0,
                        AverageSpeed = g.Sum(h => h.Flow) != 0 ? g.Sum(h => h.Average * (h.Flow ?? 0)) / g.Sum(h => h.Flow) ?? 0 : 0,
                        AverageEightyFifthSpeed = g.Sum(h => h.Flow) != 0 ? g.Sum(h => h.EightyFifthSpeed ?? 0) / g.Sum(h => h.Flow) ?? 0 : 0,
                        Variablitiy = ((double)g.Max(h => h.MaxSpeed ?? 0)) - ((double)g.Min(h => h.MinSpeed ?? 0)),
                        PercentViolations = (g.Sum(h => h.Flow) != 0 ? (double)(g.Sum(h => h.Violation ?? 0)) / g.Sum(h => h.Flow) ?? 0 : 0) * 100,
                        PercentExtremeViolations = (g.Sum(h => h.Flow) != 0 ? (double)(g.Sum(h => h.ExtremeViolation ?? 0)) / g.Sum(h => h.Flow) ?? 0 : 0) * 100,
                    })
                    .FirstOrDefault();

                var segmentSpeedViolations = new EffectivenessOfStrategiesDto
                {
                    SegmentId = segmentId,
                    SegmentName = segment.Name,
                    ChangeInAverageSpeed = aggregatedAfter.AverageSpeed - aggregatedBefore.AverageSpeed,
                    ChangeInEightyFifthPercentileSpeed = aggregatedAfter.AverageEightyFifthSpeed - aggregatedBefore.AverageEightyFifthSpeed,
                    ChangeInVariablitiy = aggregatedAfter.Variablitiy - aggregatedBefore.Variablitiy,
                    ChangeInPercentViolations = aggregatedAfter.PercentViolations - aggregatedBefore.PercentViolations,
                    ChangeInPercentExtremeViolations = aggregatedAfter.PercentExtremeViolations - aggregatedBefore.PercentExtremeViolations,
                    SpeedLimit = segment.SpeedLimit,
                    WeeklyEffectiveness = aggregatedBy7Days,
                    Before = aggregatedBefore,
                    After = aggregatedAfter
                };

                result.Add(segmentSpeedViolations);
            }
            return result;
        }

    }
}
