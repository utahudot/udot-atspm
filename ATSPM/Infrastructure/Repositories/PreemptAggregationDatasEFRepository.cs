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
            return _db.Set<PreemptionAggregation>().Where(r => r.BinStartTime >= start && r.BinStartTime < end).ToList();
        }

        public int GetPreemptAggregationTotalByVersionIdAndDateRange(DateTime start, DateTime end)
        {
            return _db.Set<PreemptionAggregation>().Where(r => r.BinStartTime >= start && r.BinStartTime < end).Select(r => r.PreemptServices).Sum();
        }

        public int GetPreemptAggregationTotalByVersionIdPreemptNumberAndDateRange(DateTime start, DateTime end, int preemptNumber)
        {
            return _db.Set<PreemptionAggregation>()
                .Where(r => r.PreemptNumber == preemptNumber && r.BinStartTime >= start && r.BinStartTime < end)
                .Select(r => r.PreemptServices)
                .Sum();
        }

        public IReadOnlyCollection<PreemptionAggregation> GetPreemptionsBySignalIdAndDateRange(string signalId, DateTime startDate, DateTime endDate)
        {
            return _db.Set<PreemptionAggregation>().Where(r => r.SignalId == signalId && r.BinStartTime >= startDate && r.BinStartTime < endDate).ToList();
        }

        PreemptionAggregation IPreemptAggregationDatasRepository.Add(PreemptionAggregation preemptionAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
