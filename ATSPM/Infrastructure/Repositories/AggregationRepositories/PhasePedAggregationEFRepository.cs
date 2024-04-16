using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IPhaseLeftTurnGapAggregationRepository"/>
    public class PhasePedAggregationEFRepository : AggregationEFRepositoryBase<PhasePedAggregation>, IPhasePedAggregationRepository
    {
        ///<inheritdoc/>
        public PhasePedAggregationEFRepository(AggregationContext db, ILogger<PhasePedAggregationEFRepository> log) : base(db, log) { }

        #region IPhasePedAggregationRepository

        #endregion
    }
}
