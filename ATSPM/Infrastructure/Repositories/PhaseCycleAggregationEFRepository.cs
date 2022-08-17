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
    public class PhaseCycleAggregationEFRepository : ATSPMRepositoryEFBase<PhaseCycleAggregation>, IPhaseCycleAggregationRepository
    {
        public PhaseCycleAggregationEFRepository(DbContext db, ILogger<PhaseCycleAggregationEFRepository> log) : base(db, log)
        {

        }

        public int GetApproachCycleCountAggregationByApproachIdAndDateRange(int versionId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<PhaseCycleAggregation> GetApproachCyclesAggregationByApproachIdAndDateRange(int approachId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<PhaseCycleAggregation> GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(string signalId, int phase, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public double GetAverageRedToRedCyclesBySignalIdPhase(string signalId, int phaseNumber, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        PhaseCycleAggregation IPhaseCycleAggregationRepository.Add(PhaseCycleAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
