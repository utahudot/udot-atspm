using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IPhaseTerminationAggregationRepository : IAsyncRepository<PhaseTerminationAggregation>
    {
        [Obsolete("Use Add in the BaseClass")]
        PhaseTerminationAggregation Add(PhaseTerminationAggregation priorityAggregation);
        [Obsolete("Use Update in the BaseClass")]
        void Update(PhaseTerminationAggregation priorityAggregation);

        [Obsolete("Use Update in the BaseClass")]
        void Remove(PhaseTerminationAggregation priorityAggregation);
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate);
        IReadOnlyCollection<PhaseTerminationAggregation> GetPhaseTerminationsAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate);
    }
}
