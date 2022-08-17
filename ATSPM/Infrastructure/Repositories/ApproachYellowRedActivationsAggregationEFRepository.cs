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
    public class ApproachYellowRedActivationsAggregationEFRepository : ATSPMRepositoryEFBase<ApproachYellowRedActivationAggregation>, IApproachYellowRedActivationsAggregationRepository
    {

        public ApproachYellowRedActivationsAggregationEFRepository(DbContext db, ILogger<ApproachYellowRedActivationsAggregationEFRepository> log) : base(db, log)
        {

        }

        public int GetApproachYellowRedActivationsCountAggregationByApproachIdAndDateRange(int versionId, DateTime start, DateTime end)
        {
            var yellowRedActivations = 0;
            if (_db.Set<ApproachYellowRedActivationAggregation>().Any(r => r.ApproachId == versionId
                                                                     && r.BinStartTime >= start &&
                                                                     r.BinStartTime <= end))
                yellowRedActivations = _db.Set<ApproachYellowRedActivationAggregation>().Where(r => r.ApproachId == versionId
                                                                                              && r.BinStartTime >=
                                                                                              start &&
                                                                                              r.BinStartTime <= end)
                    .Sum(r => r.TotalRedLightViolations);
            return yellowRedActivations;
        }

        public IReadOnlyCollection<ApproachYellowRedActivationAggregation> GetApproachYellowRedActivationssAggregationByApproachIdAndDateRange(int approachId, DateTime startDate, DateTime endDate, bool getProtectedPhase)
        {
            return _db.Set<ApproachYellowRedActivationAggregation>().Where(r => r.ApproachId == approachId
                                                                          && r.BinStartTime >= startDate &&
                                                                          r.BinStartTime <= endDate
                                                                          && r.IsProtectedPhase == getProtectedPhase).ToList();
        }
    }
}
