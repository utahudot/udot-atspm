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
            var cycles = 0;
            if (_db.Set<PhaseCycleAggregation>().Any(r => r.ApproachId == versionId
                                                       && r.BinStartTime >= start && r.BinStartTime <= end))
                cycles = _db.Set<PhaseCycleAggregation>().Where(r => r.ApproachId == versionId
                                                                  && r.BinStartTime >= start &&
                                                                  r.BinStartTime <= end)
                    .Sum(r => r.TotalRedToRedCycles);
            return cycles;
        }

        public IReadOnlyCollection<PhaseCycleAggregation> GetApproachCyclesAggregationByApproachIdAndDateRange(int approachId, DateTime start, DateTime end)
        {
            if (_db.Set<PhaseCycleAggregation>().Any(r => r.ApproachId == approachId
                                                       && r.BinStartTime >= start && r.BinStartTime <= end))
                return _db.Set<PhaseCycleAggregation>().Where(r => r.ApproachId == approachId
                                                                 && r.BinStartTime >= start &&
                                                                 r.BinStartTime <= end)
                    .ToList();
            else
                return new List<PhaseCycleAggregation>();
        }

        public IReadOnlyCollection<PhaseCycleAggregation> GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(string signalId, int phase, DateTime start, DateTime end)
        {
            if (_db.Set<PhaseCycleAggregation>().Any(r => r.SignalId == signalId
            && r.PhaseNumber == phase && r.BinStartTime >= start && r.BinStartTime <= end))
                return _db.Set<PhaseCycleAggregation>().Where(r => r.SignalId == signalId
                                                                 && r.PhaseNumber == phase
                                                                 && r.BinStartTime >= start
                                                                 && r.BinStartTime <= end)
                    .ToList();
            else
                return new List<PhaseCycleAggregation>();
        }

        public double GetAverageRedToRedCyclesBySignalIdPhase(string signalId, int phaseNumber, DateTime start, DateTime end)
        {
            if (_db.Set<PhaseCycleAggregation>().Any(r => r.SignalId == signalId && r.PhaseNumber == phaseNumber
                                                       && r.BinStartTime >= start && r.BinStartTime <= end))
                return _db.Set<PhaseCycleAggregation>().Where(r => r.SignalId == signalId
                                                            && r.PhaseNumber == phaseNumber
                                                            && r.BinStartTime >= start
                                                            && r.BinStartTime <= end)
                                                  .Average(r => r.TotalRedToRedCycles);

            else
                return 0;
        }

        PhaseCycleAggregation IPhaseCycleAggregationRepository.Add(PhaseCycleAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
