using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.ViolationsAndExtremeViolations;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.ViolationsAndExtremeViolations
{
    public class ViolationsAndExtremeViolationsService : ReportServiceBase<ViolationsAndExtremeViolationsOptions, List<ViolationsAndExtremeViolationsDto>>
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository segmentRepository;

        public ViolationsAndExtremeViolationsService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository segmentRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentRepository = segmentRepository;
        }

        public override async Task<List<ViolationsAndExtremeViolationsDto>> ExecuteAsync(ViolationsAndExtremeViolationsOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var thresholdDate = DateTime.UtcNow.AddYears(-2).AddMonths(-1);
            var result = new List<ViolationsAndExtremeViolationsDto>();
            if (parameter.StartDate < thresholdDate || parameter.StartDate > parameter.EndDate)
            {
                return null;
            }
            var hourlyAggregations = await hourlySpeedRepository.HourlyAggregationsForSegmentInTimePeriod(parameter.SegmentIds, parameter.StartDate, parameter.EndDate);
            List<Segment> segments = await segmentRepository.GetSegmentsDetailsWithEntity(parameter.SegmentIds);
            foreach (var hourlyAggregation in hourlyAggregations)
            {
                var segment = segments.Where(segment => segment.Id == hourlyAggregation.SegmentId).FirstOrDefault();
                if (segment != null)
                {
                    var speedOverDistanceDto = new ViolationsAndExtremeViolationsDto
                    {
                        SegmentId = segment.Id,
                        SegmentName = segment.Name,
                        Time = hourlyAggregation.BinStartTime,
                        StartingMilePoint = segment.StartMilePoint,
                        EndingMilePoint = segment.EndMilePoint,
                        SpeedLimit = segment.SpeedLimit,
                        Flow = hourlyAggregation.Flow ?? 0,
                        Violations = hourlyAggregation.Violation ?? 0,
                        ExteremeViolations = hourlyAggregation.ExtremeViolation ?? 0
                    };

                    result.Add(speedOverDistanceDto);
                }
            }
            return result.OrderBy(x => x.SegmentId).ToList();
        }
    }
}