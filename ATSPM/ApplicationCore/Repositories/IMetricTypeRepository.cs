using ATSPM.Application.Models;
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
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<MetricType> GetAllMetrics();

        IReadOnlyCollection<MetricType> GetAllToDisplayMetrics();

        IReadOnlyCollection<MetricType> GetAllToAggregateMetrics();
        IReadOnlyCollection<MetricType> GetBasicMetrics();

        IReadOnlyCollection<MetricType> GetMetricsByIDs(List<int> metricIDs);
        [Obsolete("Use Lookup instead")]
        MetricType GetMetricsByID(int metricID);

        IReadOnlyCollection<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment);
    }
}
