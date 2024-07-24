
using ATSPM.Application.Business.RouteSpeed;
using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Application.SpeedManagement.Business.CongestionTracking;
using ATSPM.Data.Models.SpeedManagement;
using ATSPM.Data.Models.SpeedManagement.CongestionTracking;
using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.SpeedManagementServices.CongestionTracking
{
    public class CongestionTrackingService : ICongestionTrackingService
    {

        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository routeRepository;

        public CongestionTrackingService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository routeRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.routeRepository = routeRepository;
        }

        public async Task<CongestionTrackingDto> GetReportData(CongestionTrackingOptions options)
        {
            var segment = await routeRepository.LookupAsync(options.SegmentId);
            var hourlyResult = await hourlySpeedRepository.GetHourlySpeeds(options);
            var result = ConvertToCongestionResult(hourlyResult, segment);
            return result;
        }

        private static CongestionTrackingDto ConvertToCongestionResult(List<HourlySpeed> hourlyResult, Segment segment)
        {
            var grouping = hourlyResult.GroupBy(h => h.Date);
            var data = new List<CongestionDailyDataDto>();
            foreach (var group in grouping)
            {
                var series = new CongestionSeriesData()
                {
                    Average = group.Select(h => new DataPoint<double>(h.BinStartTime, h.Average)).ToList(),
                    EightyFifth = group.Select(h => new DataPoint<int>(h.BinStartTime, h.EightyFifthSpeed.Value)).ToList(),
                };
                var dailyData = new CongestionDailyDataDto()
                {
                    Date = group.Key,
                    Series = series,
                };
                data.Add(dailyData);
            }

            return new CongestionTrackingDto() { 
                SegmentId = segment.Id,
                SegmentName = segment.Name,
                SpeedLimit = segment.SpeedLimit,
                StartingMilePoint = segment.StartMilePoint,
                EndingMilePoint= segment.EndMilePoint,
                Data = data,
            };
        }
    }
}
