using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverDistance;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedOverDistance
{
    public class SpeedOverDistanceService : ReportServiceBase<SpeedOverDistanceOptions, List<SpeedOverDistanceDto>>
    {
        private readonly IMonthlyAggregationRepository monthlyAggregationRepository;
        private readonly ISegmentRepository segmentRepository;

        public SpeedOverDistanceService(IMonthlyAggregationRepository monthlyAggregationRepository, ISegmentRepository segmentRepository)
        {
            this.monthlyAggregationRepository = monthlyAggregationRepository;
            this.segmentRepository = segmentRepository;
        }

        public override async Task<List<SpeedOverDistanceDto>> ExecuteAsync(SpeedOverDistanceOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var thresholdDate = DateTime.UtcNow.AddYears(-2).AddMonths(-1);
            var result = new List<SpeedOverDistanceDto>();
            if (parameter.StartDate < thresholdDate || parameter.StartDate > parameter.EndDate)
            {
                return null;
            }
            var monthlyAggregations = await monthlyAggregationRepository.MonthlyAggregationsForSegmentInTimePeriod(parameter.SegmentIds, parameter.StartDate, parameter.EndDate);
            var aggregationsBySegment = monthlyAggregations.GroupBy(x => x.SegmentId);
            List<Segment> segments = await segmentRepository.GetSegmentsDetailsWithEntity(parameter.SegmentIds);
            foreach (var monthlyAggregation in aggregationsBySegment)
            {
                var segment = segments.Where(segment => segment.Id == monthlyAggregation.Key).FirstOrDefault();
                if (segment != null)
                {
                    var speedData = GetSpeedData(monthlyAggregation);
                    var speedOverDistanceDto = new SpeedOverDistanceDto
                    {
                        SegmentId = segment.Id,
                        SegmentName = segment.Name,
                        SpeedLimit = segment.SpeedLimit,
                        StartingMilePoint = segment.StartMilePoint,
                        EndingMilePoint = segment.EndMilePoint,
                        StartDate = parameter.StartDate,
                        EndDate = parameter.EndDate,
                        Average = speedData.Average,
                        EightyFifth = speedData.EightyFifth
                    };

                    result.Add(speedOverDistanceDto);
                }
            }
            return result.OrderBy(x => x.StartingMilePoint).ToList();
        }

        private static (double Average, long EightyFifth) GetSpeedData(IGrouping<Guid, MonthlyAggregation> monthlyAggregation)
        {
            var aggregate = monthlyAggregation.FirstOrDefault();
            var average = aggregate.AllDayAverageSpeed.Value;
            var eightyFifth = (long)aggregate.AllDayAverageEightyFifthSpeed.Value;

            return (average, eightyFifth);
        }
    }
}
