using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public class ApproachCycleAggregationEFRepository : IApproachCycleAggregationRepository
    {
        public PhaseCycleAggregation Add(PhaseCycleAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string signalId, int phaseNumber, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public int GetApproachCycleCountAggregationByApproachIdAndDateRange(int versionId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public List<PhaseCycleAggregation> GetApproachCyclesAggregationByApproachIdAndDateRange(int approachId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public List<PhaseCycleAggregation> GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(string signalId, int phase, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public double GetAverageRedToRedCyclesBySignalIdPhase(string signalId, int phaseNumber, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public int GetCycleCountBySignalIdAndDateRange(string signalId, int phaseNumber, DateTime dateTime1, DateTime dateTime2)
        {
            throw new NotImplementedException();
        }

        public void Remove(PhaseCycleAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }

        public void Update(PhaseCycleAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
