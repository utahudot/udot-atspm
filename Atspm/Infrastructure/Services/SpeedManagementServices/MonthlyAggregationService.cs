using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverDistance;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices
{
    public class MonthlyAggregationService
    {
        private readonly IMonthlyAggregationRepository monthlyAggregationRepository;
        private readonly ISegmentRepository segmentRepository;

        public MonthlyAggregationService(IMonthlyAggregationRepository monthlyAggregationRepository, ISegmentRepository segmentRepository)
        {
            this.monthlyAggregationRepository = monthlyAggregationRepository;
            this.segmentRepository = segmentRepository;
        }

        public async Task<IReadOnlyList<MonthlyAggregation>> ListMonthlyAggregationsForSegment(Guid segmentId)
        {
            var monthlyAggregations = await monthlyAggregationRepository.SelectMonthlyAggregationBySegment(segmentId);
            return monthlyAggregations;
        }

        public async Task<List<SpeedOverDistanceDto>> MonthlyAggregationsForSegmentInTimePeriod(List<Guid> segmentIds, DateTime startDate, DateTime endDate)
        {
            var thresholdDate = DateTime.UtcNow.AddYears(-2).AddMonths(-1);
            if (startDate < thresholdDate || startDate > endDate)
            {
                return null;
            }
            var monthlyAggregations = await monthlyAggregationRepository.MonthlyAggregationsForSegmentInTimePeriod(segmentIds, startDate, endDate);
            List<Segment> segments = await segmentRepository.GetSegmentsDetails(segmentIds);
            List<SpeedOverDistanceDto> speedOverDistanceDtoList = new List<SpeedOverDistanceDto>();
            foreach (var monthlyAggregation in monthlyAggregations)
            {
                var segment = segments.Where(segment => segment.Id == monthlyAggregation.SegmentId).FirstOrDefault();
                SpeedOverDistanceDto speedOverDistanceDto = new SpeedOverDistanceDto();
                speedOverDistanceDtoList.Add(speedOverDistanceDto);
            }
            return speedOverDistanceDtoList;
        }

        public async Task UpsertMonthlyAggregation(MonthlyAggregation monthlyAggregation)
        {
            //Add the monthlyAggregation
            await monthlyAggregationRepository.UpsertMonthlyAggregationAsync(monthlyAggregation);
            //if (updatedMonthlyAggregation.Id == null)
            //{
            //    return null;
            //}
            //var monthlyAggregationId = (Guid)updatedMonthlyAggregation.Id;

            //return await GetMonthlyAggregationById(monthlyAggregationId);
        }

        public async Task UpsertMonthlyAggregations(IEnumerable<MonthlyAggregation> monthlyAggregations)
        {
            await monthlyAggregationRepository.AddRangeAsync(monthlyAggregations);
        }

        //For the DeleteOldEvents
        public async Task DeleteMonthlyAggregation(MonthlyAggregation existingMonthlyAggregation)
        {
            if (existingMonthlyAggregation.Id == null)
            {
                return;
            }
            await monthlyAggregationRepository.RemoveAsync(existingMonthlyAggregation);
        }

        //For the DeleteOldEvents
        public async Task<List<MonthlyAggregation>> AllAggregationsOverTimePeriodAsync()
        {
            var other = await monthlyAggregationRepository.AllAggregationsOverTimePeriod();
            return other;
        }

        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////

        private async Task<MonthlyAggregation> GetMonthlyAggregationById(Guid id)
        {
            var monthlyAggregation = await monthlyAggregationRepository.LookupAsync(id);
            return monthlyAggregation;
        }
    }
}