using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IPhaseCycleAggregationRepository"/>
    public class PhaseCycleAggregationEFRepository : AggregationEFRepositoryBase<PhaseCycleAggregation>, IPhaseCycleAggregationRepository
    {
        ///<inheritdoc/>
        public PhaseCycleAggregationEFRepository(EventLogContext db, ILogger<PhaseCycleAggregationEFRepository> log) : base(db, log) { }

        #region IPhaseCycleAggregationRepository

        #endregion
    }
}
