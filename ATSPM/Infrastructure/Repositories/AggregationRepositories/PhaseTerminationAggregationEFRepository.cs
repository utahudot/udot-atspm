using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IPhaseTerminationAggregationRepository"/>
    public class PhaseTerminationAggregationEFRepository : AggregationEFRepositoryBase<PhaseTerminationAggregation>, IPhaseTerminationAggregationRepository
    {
        ///<inheritdoc/>
        public PhaseTerminationAggregationEFRepository(EventLogContext db, ILogger<PhaseTerminationAggregationEFRepository> log) : base(db, log) { }

        #region IPhaseTerminationAggregationRepository

        #endregion
    }
}
