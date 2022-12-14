using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Metric Comment Repository
    /// </summary>
    public interface IMetricCommentRepository : IAsyncRepository<MetricComment>
    {
        #region ExtensionMethods

        //MetricComment GetLatestCommentForReport(string signalId, int metricId);

        #endregion

        #region Obsolete

        //IReadOnlyList<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<MetricComment> GetAllMetricComments();

        //[Obsolete("Use Lookup instead")]
        //MetricComment GetMetricCommentByMetricCommentID(int metricCommentID);

        //[Obsolete("Use Add in the BaseClass")]
        //void AddOrUpdate(MetricComment metricComment);

        //[Obsolete("Use Add in the BaseClass")]
        //void Add(MetricComment metricComment);

        //[Obsolete("Use Remove in the BaseClass")]
        //void Remove(MetricComment metricComment);

        #endregion
    }
}
