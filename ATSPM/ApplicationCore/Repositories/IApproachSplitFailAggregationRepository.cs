using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IApproachSplitFailAggregationRepository : IAsyncRepository<ApproachSplitFailAggregation>
    {
        int GetApproachSplitFailCountAggregationByApproachIdAndDateRange(int versionId, DateTime start,
            DateTime end);
        [Obsolete("Use Add in the BaseClass")]
        ApproachSplitFailAggregation Add(ApproachSplitFailAggregation priorityAggregation);
        [Obsolete("Use Update in the BaseClass")]
        void Update(ApproachSplitFailAggregation priorityAggregation);
        [Obsolete("Use Update in the BaseClass")]
        void Remove(ApproachSplitFailAggregation priorityAggregation);

        IReadOnlyCollection<ApproachSplitFailAggregation> GetApproachSplitFailsAggregationByApproachIdAndDateRange(int approachId,
            DateTime startDate, DateTime endDate, bool getProtectedPhase);
    }
}
