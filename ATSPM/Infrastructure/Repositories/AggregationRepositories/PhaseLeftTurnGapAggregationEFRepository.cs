using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IPhaseLeftTurnGapAggregationRepository"/>
    public class PhaseLeftTurnGapAggregationEFRepository : AggregationEFRepositoryBase<PhaseLeftTurnGapAggregation>, IPhaseLeftTurnGapAggregationRepository
    {
        ///<inheritdoc/>
        public PhaseLeftTurnGapAggregationEFRepository(AggregationContext db, ILogger<PhaseLeftTurnGapAggregationEFRepository> log) : base(db, log) { }

        #region IPhaseLeftTurnGapAggregationRepository

        #endregion
    }
}
