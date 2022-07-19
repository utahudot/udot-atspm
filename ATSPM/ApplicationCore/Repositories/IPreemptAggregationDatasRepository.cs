using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IPreemptAggregationDatasRepository : IAsyncRepository<PreemptionAggregation>
    {
        IReadOnlyCollection<PreemptionAggregation> GetPreemptAggregationByVersionIdAndDateRange(DateTime start,
            DateTime end);

        int GetPreemptAggregationTotalByVersionIdAndDateRange(DateTime start,
            DateTime end);


        int GetPreemptAggregationTotalByVersionIdPreemptNumberAndDateRange(DateTime start,
            DateTime end, int preemptNumber);

        [Obsolete("Use Add in the BaseClass")]
        PreemptionAggregation Add(PreemptionAggregation preemptionAggregation);
        [Obsolete("Use Update in the BaseClass")]
        void Update(PreemptionAggregation preemptionAggregation);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(PreemptionAggregation preemptionAggregation);

        IReadOnlyCollection<PreemptionAggregation> GetPreemptionsBySignalIdAndDateRange(string signalId, DateTime startDate,
            DateTime endDate);
    }
}
