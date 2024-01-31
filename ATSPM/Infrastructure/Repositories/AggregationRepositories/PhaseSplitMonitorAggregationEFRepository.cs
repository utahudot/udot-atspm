using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IPhaseSplitMonitorAggregationRepository"/>
    public class PhaseSplitMonitorAggregationEFRepository : AggregationEFRepositoryBase<PhaseSplitMonitorAggregation>, IPhaseSplitMonitorAggregationRepository
    {
        ///<inheritdoc/>
        public PhaseSplitMonitorAggregationEFRepository(EventLogContext db, ILogger<PhaseSplitMonitorAggregationEFRepository> log) : base(db, log) { }

        #region IPhaseSplitMonitorAggregationRepository

        #endregion
    }
}
