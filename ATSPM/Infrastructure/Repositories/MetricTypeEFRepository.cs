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
    public class MetricTypeEFRepository : ATSPMRepositoryEFBase<MetricType>, IMetricTypeRepository
    {
        public MetricTypeEFRepository(DbContext db, ILogger<MetricTypeEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<MetricType> GetAllMetrics()
        {
            return _db.Set<MetricType>().ToList();
        }

        public IReadOnlyCollection<MetricType> GetAllToAggregateMetrics()
        {
            return _db.Set<MetricType>().Where(m => m.ShowOnAggregationSite).ToList();
        }

        public IReadOnlyCollection<MetricType> GetAllToDisplayMetrics()
        {
            return _db.Set<MetricType>().Where(m => m.ShowOnWebsite).ToList();
        }

        public IReadOnlyCollection<MetricType> GetBasicMetrics()
        {
            var dt = _db.Set<DetectionType>().Where(d => d.DetectionTypeId == 1).FirstOrDefault();
            return dt.MetricTypes.ToList();
        }

        public MetricType GetMetricsByID(int metricID)
        {
            return _db.Set<MetricType>().Find(metricID);
        }

        public IReadOnlyCollection<MetricType> GetMetricsByIDs(List<int> metricIDs)
        {
            return _db.Set<MetricType>().Where(m => metricIDs.Contains(m.MetricId)).ToList();
        }

        public IReadOnlyCollection<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment)
        {
            return _db.Set<MetricType>().Where(m => metricComment.MetricTypeIDs.Contains(m.MetricId)).ToList();
        }
    }
}
