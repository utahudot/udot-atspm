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
    public class PreemptAggregationDatasEFRepository : ATSPMRepositoryEFBase<PreemptionAggregation>, IPreemptAggregationDatasRepository
    {
        public PreemptAggregationDatasEFRepository(DbContext db, ILogger<PreemptAggregationDatasEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<PreemptionAggregation> GetPreemptAggregationByVersionIdAndDateRange(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public int GetPreemptAggregationTotalByVersionIdAndDateRange(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public int GetPreemptAggregationTotalByVersionIdPreemptNumberAndDateRange(DateTime start, DateTime end, int preemptNumber)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<PreemptionAggregation> GetPreemptionsBySignalIdAndDateRange(string signalId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        PreemptionAggregation IPreemptAggregationDatasRepository.Add(PreemptionAggregation preemptionAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
