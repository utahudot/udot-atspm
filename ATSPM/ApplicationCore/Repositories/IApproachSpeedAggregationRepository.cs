using System;
using System.Collections.Generic;
using ATSPM.Data.Models;

namespace ATSPM.Application.Repositories
{
    public interface IApproachSpeedAggregationRepository: IAggregationRepositoryBase
    {
        List<ApproachSpeedAggregation> GetSpeedsByApproachIDandDateRange(int approachId, DateTime start, DateTime end);
        void Add(ApproachSpeedAggregation approachSpeedAggregation);
        void Remove(ApproachSpeedAggregation approachSpeedAggregation);
        void Remove(int id);
        void Update(ApproachSpeedAggregation approachSpeedAggregation);
    }
}