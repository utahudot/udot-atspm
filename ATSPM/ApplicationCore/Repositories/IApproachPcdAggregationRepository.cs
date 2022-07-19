using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IApproachPcdAggregationRepository : IAsyncRepository<ApproachPcdAggregation>
    {
        int GetApproachPcdCountAggregationByApproachIdAndDateRange(int versionId, DateTime start,
           DateTime end);
        [Obsolete("Use Add in the BaseClass")]
        ApproachPcdAggregation Add(ApproachPcdAggregation priorityAggregation);
        [Obsolete("Use Update in the BaseClass")]
        void Update(ApproachPcdAggregation priorityAggregation);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(ApproachPcdAggregation priorityAggregation);

        IReadOnlyCollection<ApproachPcdAggregation> GetApproachPcdsAggregationByApproachIdAndDateRange(int approachId,
            DateTime startDate, DateTime endDate, bool getProtectedPhase);
    }
}
