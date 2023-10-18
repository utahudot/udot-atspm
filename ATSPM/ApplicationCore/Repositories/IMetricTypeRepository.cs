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
        [Obsolete("Not required in v5.0")]
        IReadOnlyList<MetricType> GetAllToDisplayMetrics();

        [Obsolete("Not required in v5.0")]
        IReadOnlyList<MetricType> GetAllToAggregateMetrics();

        [Obsolete("Not required in v5.0")]
        IReadOnlyList<MetricType> GetBasicMetrics();

        [Obsolete("Not required in v5.0")]
        IReadOnlyList<MetricType> GetMetricsByIDs(List<int> metricIDs);

        [Obsolete("Not required in v5.0")]
        IReadOnlyList<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<MetricType> GetAllMetrics();

        //[Obsolete("Use Lookup instead")]
        //MetricType GetMetricsByID(int metricID);
    }
}
