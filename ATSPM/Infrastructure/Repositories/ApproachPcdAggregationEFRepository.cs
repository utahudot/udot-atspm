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
    public class ApproachPcdAggregationEFRepository : ATSPMRepositoryEFBase<ApproachPcdAggregation>, IApproachPcdAggregationRepository
    {

        public ApproachPcdAggregationEFRepository(DbContext db, ILogger<ApproachPcdAggregationEFRepository> log) : base(db, log)
        {

        }

        public int GetApproachPcdCountAggregationByApproachIdAndDateRange(int versionId, DateTime start, DateTime end)
        {
            var pcd = 0;
            if (_db.Set<ApproachPcdAggregation>().Any(r => r.ApproachId == versionId
                                                     && r.BinStartTime >= start && r.BinStartTime <= end))
                pcd = _db.Set<ApproachPcdAggregation>().Where(r => r.ApproachId == versionId
                                                             && r.BinStartTime >= start &&
                                                             r.BinStartTime <= end)
                    .Sum(r => r.ArrivalsOnGreen);
            return pcd;
        }

        public IReadOnlyCollection<ApproachPcdAggregation> GetApproachPcdsAggregationByApproachIdAndDateRange(int approachId, DateTime startDate, DateTime endDate, bool getProtectedPhase)
        {
            return _db.Set<ApproachPcdAggregation>().Where(r => r.ApproachId == approachId
                                                          && r.BinStartTime >= startDate &&
                                                          r.BinStartTime <= endDate
                                                          && r.IsProtectedPhase == getProtectedPhase).ToList();
        }

        ApproachPcdAggregation IApproachPcdAggregationRepository.Add(ApproachPcdAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
