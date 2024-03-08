using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IApproachSpeedAggregationRepository"/>
    public class ApproachSpeedAggregationEFRepository : AggregationEFRepositoryBase<ApproachSpeedAggregation>, IApproachSpeedAggregationRepository
    {
        ///<inheritdoc/>
        public ApproachSpeedAggregationEFRepository(EventLogContext db, ILogger<ApproachSpeedAggregationEFRepository> log) : base(db, log) { }

        #region IApproachSpeedAggregationRepository

        #endregion
    }
}
