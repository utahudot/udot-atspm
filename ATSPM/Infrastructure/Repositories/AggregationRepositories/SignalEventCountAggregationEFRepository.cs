using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="ISignalEventCountAggregationRepository"/>
    public class SignalEventCountAggregationEFRepository : AggregationEFRepositoryBase<SignalEventCountAggregation>, ISignalEventCountAggregationRepository
    {
        ///<inheritdoc/>
        public SignalEventCountAggregationEFRepository(EventLogContext db, ILogger<SignalEventCountAggregationEFRepository> log) : base(db, log) { }

        #region ISignalEventCountAggregationRepository

        #endregion
    }
}
