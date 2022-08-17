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
    public class ApproachSplitFailAggregationEFRepository : ATSPMRepositoryEFBase<ApproachSplitFailAggregation>, IApproachSplitFailAggregationRepository
    {

        public ApproachSplitFailAggregationEFRepository(DbContext db, ILogger<ApproachSplitFailAggregationEFRepository> log) : base(db, log)
        {

        }

        public int GetApproachSplitFailCountAggregationByApproachIdAndDateRange(int versionId, DateTime start, DateTime end)
        {
            var splitFails = 0;
            if (_db.Set<ApproachSplitFailAggregation>().Any(r => r.ApproachId == versionId
                                                           && r.BinStartTime >= start && r.BinStartTime <= end))
                splitFails = _db.Set<ApproachSplitFailAggregation>().Where(r => r.ApproachId == versionId
                                                                          && r.BinStartTime >= start &&
                                                                          r.BinStartTime <= end)
                    .Sum(r => r.SplitFailures);
            return splitFails;
        }

        public IReadOnlyCollection<ApproachSplitFailAggregation> GetApproachSplitFailsAggregationByApproachIdAndDateRange(int approachId, DateTime startDate, DateTime endDate, bool getProtectedPhase)
        {
            return _db.Set<ApproachSplitFailAggregation>().Where(r => r.ApproachId == approachId
                                                                && r.BinStartTime >= startDate &&
                                                                r.BinStartTime <= endDate
                                                                && r.IsProtectedPhase == getProtectedPhase).ToList();
        }

        ApproachSplitFailAggregation IApproachSplitFailAggregationRepository.Add(ApproachSplitFailAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
