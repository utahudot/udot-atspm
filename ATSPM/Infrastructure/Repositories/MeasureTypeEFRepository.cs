using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Measure type entity framework repository
    /// </summary>
    public class MeasureTypeEFRepository : ATSPMRepositoryEFBase<MeasureType>, IMeasureTypeRepository
    {
        /// <inheritdoc/>
        public MeasureTypeEFRepository(ConfigContext db, ILogger<MeasureTypeEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region IMeasureTypeRepository

        //[Obsolete("Not required in v5.0")]
        //IReadOnlyList<MeasureType> GetAllToDisplayMetrics();

        //[Obsolete("Not required in v5.0")]
        //IReadOnlyList<MeasureType> GetAllToAggregateMetrics();

        //[Obsolete("Not required in v5.0")]
        //IReadOnlyList<MeasureType> GetBasicMetrics();

        //[Obsolete("Not required in v5.0")]
        //IReadOnlyList<MeasureType> GetMetricsByIDs(List<int> metricIDs);

        //[Obsolete("Not required in v5.0")]
        //IReadOnlyList<MeasureType> GetMetricTypesByMetricComment(MeasureComment metricComment);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<MetricType> GetAllMetrics();

        //[Obsolete("Use Lookup instead")]
        //MetricType GetMetricsByID(int metricID);

        #endregion
    }
}
