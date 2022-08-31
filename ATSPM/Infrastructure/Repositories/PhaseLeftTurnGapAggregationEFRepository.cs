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
    public class PhaseLeftTurnGapAggregationEFRepository : ATSPMRepositoryEFBase<PhaseLeftTurnGapAggregation>, IPhaseLeftTurnGapAggregationRepository
    {
        public PhaseLeftTurnGapAggregationEFRepository(DbContext db, ILogger<PhaseLeftTurnGapAggregationEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<PhaseLeftTurnGapAggregation> GetAggregationByApproachIdAndDateRange(int approachID, DateTime startDate, DateTime endDate)
        {
            if (_db.Set<PhaseLeftTurnGapAggregation>().Any(r => r.ApproachId == approachID
                                                      && r.BinStartTime >= startDate && r.BinStartTime <= endDate))
                return _db.Set<PhaseLeftTurnGapAggregation>().Where(r => r.ApproachId == approachID
                                                                 && r.BinStartTime >= startDate &&
                                                                 r.BinStartTime <= endDate)
                    .ToList();
            else
                return new List<PhaseLeftTurnGapAggregation>();
        }

        public IReadOnlyCollection<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate)
        {
            return _db.Set<PhaseLeftTurnGapAggregation>().Where(p =>
                    p.SignalId == signal.SignalId && p.BinStartTime >= startDate &&
                    p.BinStartTime < endDate)
                .Select(p => p.PhaseNumber).Distinct().ToList();
        }

        public IReadOnlyCollection<PhaseLeftTurnGapAggregation> GetPhaseLeftTurnGapAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            return _db.Set<PhaseLeftTurnGapAggregation>().Where(p =>
                p.SignalId == signalId && p.PhaseNumber == phaseNumber && p.BinStartTime >= startDate &&
                p.BinStartTime < endDate).ToList();
        }

        public int GetSummedGapsBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate, int gapCountColumn)
        {
            var query = _db.Set<PhaseLeftTurnGapAggregation>().Where(p =>
                p.SignalId == signalId && p.PhaseNumber == phaseNumber && p.BinStartTime >= startDate &&
                p.BinStartTime < endDate);
            switch (gapCountColumn)
            {
                case 1: return query.Sum(p => p.GapCount1);
                case 2: return query.Sum(p => p.GapCount2);
                case 3: return query.Sum(p => p.GapCount3);
                case 4: return query.Sum(p => p.GapCount4);
                case 5: return query.Sum(p => p.GapCount5);
                case 6: return query.Sum(p => p.GapCount6);
                case 7: return query.Sum(p => p.GapCount7);
                case 8: return query.Sum(p => p.GapCount8);
                case 9: return query.Sum(p => p.GapCount9);
                case 10: return query.Sum(p => p.GapCount10);
                case 11: return query.Sum(p => p.GapCount11);
                default: throw new Exception("Gap Column not found");
            }
        }

        PhaseLeftTurnGapAggregation IPhaseLeftTurnGapAggregationRepository.Add(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
