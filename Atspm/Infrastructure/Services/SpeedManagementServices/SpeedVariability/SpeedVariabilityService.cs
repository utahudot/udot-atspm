using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedVariability;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedVariability
{
    public class SpeedVariabilityService : ReportServiceBase<SpeedVariabilityOptions, SpeedVariabilityDto>
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository segmentRepository;
        private readonly IMonthlyAggregationRepository monthlyAggregationRepository;

        public SpeedVariabilityService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository segmentRepository, IMonthlyAggregationRepository monthlyAggregationRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentRepository = segmentRepository;
            this.monthlyAggregationRepository = monthlyAggregationRepository;
        }

        public override async Task<SpeedVariabilityDto> ExecuteAsync(SpeedVariabilityOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var segment = await segmentRepository.LookupAsync(parameter.SegmentId);
            var hourlySpeeds = await hourlySpeedRepository.GetHourlySpeedsForSegmentInSource(parameter, segment.Id);

            if (hourlySpeeds == null || hourlySpeeds.Count == 0)
            {
                throw new Exception($"No data found for {segment.Name} in source");
            }

            var groupedByDate = hourlySpeeds
                .GroupBy(hs => hs.Date.Date)
                .Select(g =>
                {
                    var min = g.Min(hs => hs.Average);
                    var max = g.Max(hs => hs.Average);
                    var variability = max - min;
                    return new SpeedVariabilityDataDto
                    {
                        Date = g.Key,
                        AvgSpeed = GetWeightedAverageSpeed(g.ToList()),
                        MinSpeed = Math.Round(min, 2),
                        MaxSpeed = Math.Round(max, 2),
                        SpeedVariability = Math.Round(variability, 2)
                    };
                }).ToList();

            return new SpeedVariabilityDto()
            {
                SegmentId = segment.Id,
                SegmentName = segment.Name,
                StartDate = parameter.StartDate.ToDateTime(new TimeOnly(0,0)),
                EndDate = parameter.EndDate.ToDateTime(new TimeOnly(0, 0)),
                StartingMilePoint = segment.StartMilePoint,
                EndingMilePoint = segment.EndMilePoint,
                SpeedLimit = segment.SpeedLimit,
                speedVariabilities = groupedByDate,
            };
        }

        private static double GetWeightedAverageSpeed(List<HourlySpeed> hourlySpeeds)
        {
            if (hourlySpeeds == null || hourlySpeeds.Count() == 0)
            {
                return 0;
            }
            double sumFlow = hourlySpeeds.Sum(hs => hs.Flow ?? 0);
            //if flow is all zero do normal average
            if (sumFlow <= 0)
            {
                double totalAverage = hourlySpeeds.Sum(hs => hs.Average);
                return Math.Round(totalAverage / hourlySpeeds.Count(), 2);
            }
            var flowSpeed = new List<double>();
            foreach (var hourlySpeed in hourlySpeeds)
            {
                double flow = hourlySpeed.Flow ?? 0;
                double speed = hourlySpeed.Average;
                var flowAndSpeed = flow * speed;
                flowSpeed.Add(flowAndSpeed);
            }
            var totalFlowAndSpeed = flowSpeed.Sum();
            return Math.Round(totalFlowAndSpeed / sumFlow, 2);
        }
    }
}
