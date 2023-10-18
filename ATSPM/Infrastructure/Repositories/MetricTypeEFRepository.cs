using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ATSPM.Infrastructure.Repositories
{
    public class MetricTypeEFRepository : ATSPMRepositoryEFBase<MetricType>, IMetricTypeRepository
    {
        public MetricTypeEFRepository(ConfigContext db, ILogger<MetricTypeEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region IMetricCommentRepository

        public IReadOnlyList<MetricType> GetAllToAggregateMetrics()
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<MetricType> GetAllToDisplayMetrics()
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<MetricType> GetBasicMetrics()
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<MetricType> GetMetricsByIDs(List<int> metricIDs)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
