using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IPriorityAggregationRepository"/>
    public class PriorityAggregationEFRepository : AggregationEFRepositoryBase<PriorityAggregation>, IPriorityAggregationRepository
    {
        ///<inheritdoc/>
        public PriorityAggregationEFRepository(EventLogContext db, ILogger<PriorityAggregationEFRepository> log) : base(db, log) { }

        #region IPriorityAggregationRepository

        #endregion
    }
}
