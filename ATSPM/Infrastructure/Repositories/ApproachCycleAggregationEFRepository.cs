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
    public class ApproachCycleAggregationEFRepository : ATSPMRepositoryEFBase<ApproachCycleAggregation>, IApproachCycleAggregationRepository
    {
        public ApproachCycleAggregationEFRepository(DbContext db, ILogger<ApproachCycleAggregationEFRepository> log) : base(db, log)
        {

        }

        PhaseCycleAggregation IApproachCycleAggregationRepository.Add(PhaseCycleAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }

        bool IApproachCycleAggregationRepository.Exists(string signalId, int phaseNumber, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        int IApproachCycleAggregationRepository.GetApproachCycleCountAggregationByApproachIdAndDateRange(int versionId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        IReadOnlyCollection<PhaseCycleAggregation> IApproachCycleAggregationRepository.GetApproachCyclesAggregationByApproachIdAndDateRange(int approachId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        IReadOnlyCollection<PhaseCycleAggregation> IApproachCycleAggregationRepository.GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(string signalId, int phase, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        double IApproachCycleAggregationRepository.GetAverageRedToRedCyclesBySignalIdPhase(string signalId, int phaseNumber, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        int IApproachCycleAggregationRepository.GetCycleCountBySignalIdAndDateRange(string signalId, int phaseNumber, DateTime dateTime1, DateTime dateTime2)
        {
            throw new NotImplementedException();
        }

        void IApproachCycleAggregationRepository.Remove(PhaseCycleAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }

        void IApproachCycleAggregationRepository.Update(PhaseCycleAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
