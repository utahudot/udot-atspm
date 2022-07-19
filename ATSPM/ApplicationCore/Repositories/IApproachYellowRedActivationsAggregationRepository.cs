using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IApproachYellowRedActivationsAggregationRepository : IAsyncRepository<ApproachYellowRedActivationAggregation>
    {
        int GetApproachYellowRedActivationsCountAggregationByApproachIdAndDateRange(int versionId, DateTime start,
            DateTime end);
        [Obsolete("Use Add in the BaseClass")]
        //YellowRedActivationsAggregationByApproach Add(YellowRedActivationsAggregationByApproach priorityAggregation);
        //[Obsolete("Use Update in the BaseClass")]
        //void Update(YellowRedActivationsAggregationByApproach priorityAggregation);
        //[Obsolete("Use Remove in the BaseClass")]
        //void Remove(YellowRedActivationsAggregationByApproach priorityAggregation);

        IReadOnlyCollection<ApproachYellowRedActivationAggregation>
            GetApproachYellowRedActivationssAggregationByApproachIdAndDateRange(int approachId, DateTime startDate,
                DateTime endDate, bool getProtectedPhase);
    }
}
