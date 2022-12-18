using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IMetricTypeRepository : IAsyncRepository<MetricType>
    {
        IReadOnlyList<MetricType> GetAllToDisplayMetrics();

        IReadOnlyList<MetricType> GetAllToAggregateMetrics();
        
        IReadOnlyList<MetricType> GetBasicMetrics();

        IReadOnlyList<MetricType> GetMetricsByIDs(List<int> metricIDs);

        IReadOnlyList<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment);

        [Obsolete("Use GetList instead")]
        IReadOnlyList<MetricType> GetAllMetrics();

        [Obsolete("Use Lookup instead")]
        MetricType GetMetricsByID(int metricID);
    }
}
