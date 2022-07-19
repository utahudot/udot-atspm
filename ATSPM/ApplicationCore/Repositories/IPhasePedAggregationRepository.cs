using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IPhasePedAggregationRepository : IAsyncRepository<PhasePedAggregation>
    {
        [Obsolete("Use Add in the BaseClass")]
        PhasePedAggregation Add(PhasePedAggregation pedAggregation);
        [Obsolete("Use Update in the BaseClass")]
        void Update(PhasePedAggregation pedAggregation);
        [Obsolete("Use Removein the BaseClass")]
        void Remove(PhasePedAggregation pedAggregation);
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate);
        IReadOnlyCollection<PhasePedAggregation> GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate);
    }
}
