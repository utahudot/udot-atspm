using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="ISignalPlanAggregationRepository"/>
    public class SignalPlanAggregationEFRepository : AggregationEFRepositoryBase<SignalPlanAggregation>, ISignalPlanAggregationRepository
    {
        ///<inheritdoc/>
        public SignalPlanAggregationEFRepository(EventLogContext db, ILogger<SignalPlanAggregationEFRepository> log) : base(db, log) { }

        #region ISignalPlanAggregationRepository

        #endregion
    }
}
