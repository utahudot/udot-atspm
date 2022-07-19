using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IMetricCommentRepository : IAsyncRepository<MetricComment> 
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<MetricComment> GetAllMetricComments();
        [Obsolete("Use Lookup instead")]
        MetricComment GetMetricCommentByMetricCommentID(int metricCommentID);
        [Obsolete("Use Add in the BaseClass")]
        void AddOrUpdate(MetricComment metricComment);
        [Obsolete("Use Add in the BaseClass")]
        void Add(MetricComment metricComment);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(MetricComment metricComment);
        MetricComment GetLatestCommentForReport(string signalID, int metricID);
        IReadOnlyCollection<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment);
    }
}
