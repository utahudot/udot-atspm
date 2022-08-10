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

        public ApproachYellowRedActivationsAggregationEFRepository(DbContext db, ILogger<ApproachEFRepository> log) : base(db, log)
        {

        }

        public int GetApproachYellowRedActivationsCountAggregationByApproachIdAndDateRange(int versionId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<ApproachYellowRedActivationAggregation> GetApproachYellowRedActivationssAggregationByApproachIdAndDateRange(int approachId, DateTime startDate, DateTime endDate, bool getProtectedPhase)
        {
            throw new NotImplementedException();
        }
    }
}
