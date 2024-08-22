using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagementAggregation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.SpeedManagementServices
{
    public class HourlySpeedService
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository segmentRepository;

        public HourlySpeedService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository segmentRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentRepository = segmentRepository;
        }

        public async Task<List<HourlySpeed>> GetHourlySpeedsForTimePeriod(Guid segmentId, DateTime startDate, DateTime endDate, DateTime startTime, DateTime endTime)
        {
            var hourlySpeeds = await hourlySpeedRepository.GetHourlySpeedsForTimePeriod(segmentId, startDate, endDate, startTime, endTime);
            return hourlySpeeds;
        }


        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////

    }
}