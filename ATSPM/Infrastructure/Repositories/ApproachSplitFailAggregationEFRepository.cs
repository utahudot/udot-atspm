using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public class ApproachSplitFailAggregationEFRepository : IApproachSplitFailAggregationRepository
    {
        public ApproachSplitFailAggregation Add(ApproachSplitFailAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public int GetApproachSplitFailCountAggregationByApproachIdAndDateRange(int versionId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public List<ApproachSplitFailAggregation> GetApproachSplitFailsAggregationByApproachIdAndDateRange(int approachId, DateTime startDate, DateTime endDate, bool getProtectedPhase)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetLastAggregationDate()
        {
            throw new NotImplementedException();
        }

        public void Remove(ApproachSplitFailAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }

        public void Update(ApproachSplitFailAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
