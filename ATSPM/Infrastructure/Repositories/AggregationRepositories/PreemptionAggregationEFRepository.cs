using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IPreemptionAggregationRepository"/>
    public class PreemptionAggregationEFRepository : AggregationEFRepositoryBase<PreemptionAggregation>, IPreemptionAggregationRepository
    {
        ///<inheritdoc/>
        public PreemptionAggregationEFRepository(EventLogContext db, ILogger<PreemptionAggregationEFRepository> log) : base(db, log) { }

        #region IPreemptionAggregationRepository

        #endregion
    }
}
