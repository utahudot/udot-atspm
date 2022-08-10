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

        ApproachPcdAggregation IApproachPcdAggregationRepository.Add(ApproachPcdAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }

        int IApproachPcdAggregationRepository.GetApproachPcdCountAggregationByApproachIdAndDateRange(int versionId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        IReadOnlyCollection<ApproachPcdAggregation> IApproachPcdAggregationRepository.GetApproachPcdsAggregationByApproachIdAndDateRange(int approachId, DateTime startDate, DateTime endDate, bool getProtectedPhase)
        {
            throw new NotImplementedException();
        }

        void IApproachPcdAggregationRepository.Remove(ApproachPcdAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }

        void IApproachPcdAggregationRepository.Update(ApproachPcdAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
