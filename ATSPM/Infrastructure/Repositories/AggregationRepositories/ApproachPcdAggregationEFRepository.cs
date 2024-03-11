using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IApproachPcdAggregationRepository"/>
    public class ApproachPcdAggregationEFRepository : AggregationEFRepositoryBase<ApproachPcdAggregation>, IApproachPcdAggregationRepository
    {
        ///<inheritdoc/>
        public ApproachPcdAggregationEFRepository(AggregationContext db, ILogger<ApproachPcdAggregationEFRepository> log) : base(db, log) { }

        #region IApproachPcdAggregationRepository

        #endregion
    }
}
