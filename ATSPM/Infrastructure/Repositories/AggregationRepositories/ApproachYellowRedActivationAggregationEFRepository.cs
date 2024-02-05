using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IApproachYellowRedActivationAggregationRepository"/>
    public class ApproachYellowRedActivationAggregationEFRepository : AggregationEFRepositoryBase<ApproachYellowRedActivationAggregation>, IApproachYellowRedActivationAggregationRepository
    {
        ///<inheritdoc/>
        public ApproachYellowRedActivationAggregationEFRepository(EventLogContext db, ILogger<ApproachYellowRedActivationAggregationEFRepository> log) : base(db, log) { }

        #region IApproachYellowRedActivationAggregationRepository

        #endregion
    }
}
