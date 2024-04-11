using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IDetectorEventCountAggregationRepository"/>
    public class DetectorEventCountAggregationEFRepository : AggregationEFRepositoryBase<DetectorEventCountAggregation>, IDetectorEventCountAggregationRepository
    {
        ///<inheritdoc/>
        public DetectorEventCountAggregationEFRepository(AggregationContext db, ILogger<DetectorEventCountAggregationEFRepository> log) : base(db, log) { }

        #region IDetectorEventCountAggregationRepository

        #endregion
    }
}
