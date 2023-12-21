using System;
using System.Collections.Generic;
using ATSPM.Data.Models;

namespace ATSPM.Application.Repositories
{
    public interface IApproachSplitFailAggregationRepository : IAggregationRepositoryBase
    {
        int GetApproachSplitFailCountAggregationByApproachIdAndDateRange(int versionId, DateTime start,
            DateTime end);

        ApproachSplitFailAggregation Add(ApproachSplitFailAggregation priorityAggregation);
        void Update(ApproachSplitFailAggregation priorityAggregation);
        void Remove(ApproachSplitFailAggregation priorityAggregation);

        List<ApproachSplitFailAggregation> GetApproachSplitFailsAggregationByApproachIdAndDateRange(int approachId,
            DateTime startDate, DateTime endDate, bool getProtectedPhase);
        bool Exists(string locationId, int phaseNumber, DateTime startDate, DateTime endDate);
    }
}