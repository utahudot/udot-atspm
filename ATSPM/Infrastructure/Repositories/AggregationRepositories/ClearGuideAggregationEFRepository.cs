using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="ISignalEventCountAggregationRepository"/>
    public class ClearGuidAggregationEFRepository : AggregationEFRepositoryBase<ClearGuideAggregation>, IClearguideAggregationRepository
    {
        ///<inheritdoc/>
        public ClearGuidAggregationEFRepository(AggregationContext db, ILogger<ClearGuidAggregationEFRepository> log) : base(db, log) { }

        #region ISignalEventCountAggregationRepository

        #endregion
    }
}
