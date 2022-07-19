using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IPriorityAggregationDatasRepository : IAsyncRepository<PriorityAggregation>
    {
        IReadOnlyCollection<PriorityAggregation> GetPriorityAggregationByVersionIdAndDateRange(int versionId, DateTime start,
           DateTime end);
        [Obsolete("Use Add in the BaseClass")]
        PriorityAggregation Add(PriorityAggregation priorityAggregation);
        [Obsolete("Use Update in the BaseClass")]
        void Update(PriorityAggregation priorityAggregation);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(PriorityAggregation priorityAggregation);
        IReadOnlyCollection<PriorityAggregation> GetPriorityBySignalIdAndDateRange(string signalId, DateTime start, DateTime end);
    }
}
