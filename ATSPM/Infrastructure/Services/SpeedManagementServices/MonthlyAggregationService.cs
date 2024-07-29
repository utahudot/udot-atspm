using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagementAggregation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.SpeedManagementServices
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

        public async Task<MonthlyAggregation> UpsertMonthlyAggregation(MonthlyAggregation monthlyAggregation)
        {
            //Add the monthlyAggregation
            var updatedMonthlyAggregation = await monthlyAggregationRepository.UpsertMonthlyAggregationAsync(monthlyAggregation);
            if (updatedMonthlyAggregation.Id == null)
            {
                return null;
            }
            var monthlyAggregationId = (Guid)updatedMonthlyAggregation.Id;

            return await GetMonthlyAggregationById(monthlyAggregationId);
        }

        public async Task DeleteMonthlyAggregation(MonthlyAggregation existingMonthlyAggregation)
        {
            if (existingMonthlyAggregation.Id == null)
            {
                return;
            }
            await monthlyAggregationRepository.RemoveAsync(existingMonthlyAggregation);
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