using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IApproachSplitFailAggregationRepository"/>
    public class ApproachSplitFailAggregationEFRepository : AggregationEFRepositoryBase<ApproachSplitFailAggregation>, IApproachSplitFailAggregationRepository
    {
        ///<inheritdoc/>
        public ApproachSplitFailAggregationEFRepository(AggregationContext db, ILogger<ApproachSplitFailAggregationEFRepository> log) : base(db, log) { }

        #region IApproachSplitFailAggregationRepository

        #endregion
    }
}
