using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IApproachSpeedAggregationRepository : IAsyncRepository<ApproachSpeedAggregation>
    {
        IReadOnlyCollection<ApproachSpeedAggregation> GetSpeedsByApproachIDandDateRange(int approachId, DateTime start, DateTime end);
        [Obsolete("Use Add in the BaseClass")]
        void Add(ApproachSpeedAggregation approachSpeedAggregation);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(ApproachSpeedAggregation approachSpeedAggregation);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(int id);
        [Obsolete("Use Update in the BaseClass")]
        void Update(ApproachSpeedAggregation approachSpeedAggregation);
    }
}
