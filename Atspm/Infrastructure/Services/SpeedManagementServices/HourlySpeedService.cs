using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices
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