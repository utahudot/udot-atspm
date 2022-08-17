using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class MetricCommentEFRepository : ATSPMRepositoryEFBase<MetricComment>, IMetricCommentRepository
    {
        public MetricCommentEFRepository(DbContext db, ILogger<MetricCommentEFRepository> log) : base(db, log)
        {

        }

        public void AddOrUpdate(MetricComment metricComment)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<MetricComment> GetAllMetricComments()
        {
            return _db.Set<MetricComment>().ToList();
        }

        public MetricComment GetLatestCommentForReport(string signalID, int metricID)
        {
            var comments = _db.Set<MetricComment>().Where(mc => mc.SignalId == signalID).OrderByDescending(x => x.TimeStamp).ToList();

            var commentsForMetricType = new List<MetricComment>();
            if (comments != null)
                foreach (var mc in comments)
                    foreach (var mt in mc.MetricCommentMetricTypes)
                        if (mt.MetricTypeMetricId == metricID)
                        {
                            commentsForMetricType.Add(mc);
                            break;
                        }

            return commentsForMetricType.FirstOrDefault();
        }

        public MetricComment GetMetricCommentByMetricCommentID(int metricCommentID)
        {
            return _db.Set<MetricComment>().Where(mc => mc.CommentId == metricCommentID).FirstOrDefault();
        }

        public IReadOnlyCollection<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment)
        {
            return _db.Set<MetricType>().Where(mc => metricComment.MetricTypeIDs.Contains(mc.MetricId)).ToList();
        }
    }
}
