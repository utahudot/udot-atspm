using ATSPM.Application.Business;
using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement;
using ATSPM.Data.Models.SpeedManagement.Common;
using ATSPM.Data.Models.SpeedManagement.CongestionTracking;
using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


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
            var hourlyResult = await hourlySpeedRepository.GetHourlySpeeds(parameter.StartDate, parameter.EndDate, Guid.Parse(parameter.SegmentId));
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
