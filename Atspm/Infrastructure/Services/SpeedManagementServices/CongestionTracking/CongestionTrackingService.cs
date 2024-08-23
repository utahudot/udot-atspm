using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.CongestionTracking;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;


namespace ATSPM.Infrastructure.Services.SpeedManagementServices.CongestionTracking
{
    public class CongestionTrackingService : ReportServiceBase<CongestionTrackingOptions, CongestionTrackingDto>
    {

        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository segmentRepository;

        public CongestionTrackingService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository routeRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentRepository = routeRepository;
        }
        public override async Task<CongestionTrackingDto> ExecuteAsync(CongestionTrackingOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var segment = await segmentRepository.LookupAsync(parameter.SegmentId);
            var hourlyResult = await hourlySpeedRepository.GetHourlySpeedsForSegmentInSource(parameter, Guid.Parse(parameter.SegmentId));
            var result = ConvertToCongestionResult(hourlyResult, segment);
            return result;
        }

        private static CongestionTrackingDto ConvertToCongestionResult(List<HourlySpeed> hourlyResult, Segment segment)
        {
            var grouping = hourlyResult.GroupBy(h => h.Date);
            var data = new List<SpeedDataDto>();

            foreach (var group in grouping)
            {
                var averageData = group.Select(h => new DataPoint<double>(h.BinStartTime, h.Average)).ToList();
                var eightyFifthData = group.Select(h => h.EightyFifthSpeed).All(speed => speed == null)
                    ? null
                    : group.Select(h => new DataPoint<long>(h.BinStartTime, (long)(h.EightyFifthSpeed ?? 0))).ToList();

                var series = new AverageAndEightyFifthSeriesData()
                {
                    Average = averageData,
                    EightyFifth = eightyFifthData,
                };

                var dailyData = new SpeedDataDto()
                {
                    Date = group.Key,
                    Series = series,
                };

                data.Add(dailyData);
            }

            return new CongestionTrackingDto()
            {
                SegmentId = segment.Id,
                SegmentName = segment.Name,
                SpeedLimit = segment.SpeedLimit,
                StartingMilePoint = segment.StartMilePoint,
                EndingMilePoint = segment.EndMilePoint,
                Data = data,
            };
        }
    }
}
