using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IPhaseCycleAggregationRepository : IAsyncRepository<PhaseCycleAggregation>
    {
        int GetApproachCycleCountAggregationByApproachIdAndDateRange(int versionId, DateTime start,
            DateTime end);
        [Obsolete("Use Add in the BaseClass")]
        PhaseCycleAggregation Add(PhaseCycleAggregation priorityAggregation);
        [Obsolete("Use Update in the BaseClass")]
        void Update(PhaseCycleAggregation priorityAggregation);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(PhaseCycleAggregation priorityAggregation);
        IReadOnlyCollection<PhaseCycleAggregation> GetApproachCyclesAggregationByApproachIdAndDateRange(int approachId, DateTime start,
            DateTime end);
        double GetAverageRedToRedCyclesBySignalIdPhase(string signalId, int phaseNumber, DateTime start,
            DateTime end);
        IReadOnlyCollection<PhaseCycleAggregation> GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(string signalId, int phase, DateTime start,
            DateTime end);
    }
}
