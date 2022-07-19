using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IPhaseSplitMonitorAggregationRepository : IAsyncRepository<PhaseSplitMonitorAggregation>
    {
        [Obsolete("Use Add in the BaseClass")]
        PhaseSplitMonitorAggregation Add(PhaseSplitMonitorAggregation splitMonitorAggregation);
        [Obsolete("Use Update in the BaseClass")]
        void Update(PhaseSplitMonitorAggregation splitMonitorAggregation);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(PhaseSplitMonitorAggregation splitMonitorAggregation);
        IReadOnlyCollection<PhaseSplitMonitorAggregation> GetSplitMonitorAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate);
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate);
    }
}
