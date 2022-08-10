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
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<ApproachSplitFailAggregation> GetApproachSplitFailsAggregationByApproachIdAndDateRange(int approachId, DateTime startDate, DateTime endDate, bool getProtectedPhase)
        {
            throw new NotImplementedException();
        }

        ApproachSplitFailAggregation IApproachSplitFailAggregationRepository.Add(ApproachSplitFailAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
