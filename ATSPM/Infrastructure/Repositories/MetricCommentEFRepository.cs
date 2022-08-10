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
            throw new NotImplementedException();
        }

        public MetricComment GetLatestCommentForReport(string signalID, int metricID)
        {
            throw new NotImplementedException();
        }

        public MetricComment GetMetricCommentByMetricCommentID(int metricCommentID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment)
        {
            throw new NotImplementedException();
        }
    }
}
