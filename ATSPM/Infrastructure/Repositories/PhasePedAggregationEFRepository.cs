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
    public class PhasePedAggregationEFRepository : ATSPMRepositoryEFBase<PhasePedAggregation>, IPhasePedAggregationRepository
    {
        public PhasePedAggregationEFRepository(DbContext db, ILogger<PhasePedAggregationEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate)
        {
            return _db.Set<PhasePedAggregation>()
                .Where(p => p.SignalId == signal.SignalId && p.BinStartTime >= startDate && p.BinStartTime < endDate)
                .Select(p => p.PhaseNumber)
                .ToList();
        }

        public IReadOnlyCollection<PhasePedAggregation> GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            return _db.Set<PhasePedAggregation>()
                .Where(p => p.SignalId == signalId && p.PhaseNumber == phaseNumber && p.BinStartTime >= startDate && p.BinStartTime < endDate)
                .ToList();
        }

        PhasePedAggregation IPhasePedAggregationRepository.Add(PhasePedAggregation pedAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
