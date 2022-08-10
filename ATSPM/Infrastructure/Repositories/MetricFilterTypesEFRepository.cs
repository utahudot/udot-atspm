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
    public class MetricFilterTypesEFRepository : ATSPMRepositoryEFBase<MetricsFilterType>, IMetricFilterTypesRepository
    {
        public MetricFilterTypesEFRepository(DbContext db, ILogger<MetricFilterTypesEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<MetricsFilterType> GetAllFilters()
        {
            throw new NotImplementedException();
        }
    }
}
