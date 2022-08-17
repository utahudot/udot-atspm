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
    public class PriorityAggregationDatasEFRepository : ATSPMRepositoryEFBase<PriorityAggregation>, IPriorityAggregationDatasRepository
    {
        public PriorityAggregationDatasEFRepository(DbContext db, ILogger<PriorityAggregationDatasEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<PriorityAggregation> GetPriorityAggregationByVersionIdAndDateRange(int versionId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<PriorityAggregation> GetPriorityBySignalIdAndDateRange(string signalId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        PriorityAggregation IPriorityAggregationDatasRepository.Add(PriorityAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
