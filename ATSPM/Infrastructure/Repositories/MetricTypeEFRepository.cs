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
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<MetricType> GetAllToAggregateMetrics()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<MetricType> GetAllToDisplayMetrics()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<MetricType> GetBasicMetrics()
        {
            throw new NotImplementedException();
        }

        public MetricType GetMetricsByID(int metricID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<MetricType> GetMetricsByIDs(List<int> metricIDs)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment)
        {
            throw new NotImplementedException();
        }
    }
}
