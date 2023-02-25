using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public class PhaseLeftTurnGapAggregationEFRepository : IPhaseLeftTurnGapAggregationRepository
    {
        public PhaseLeftTurnGapAggregation Add(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public List<PhaseLeftTurnGapAggregation> GetAggregationByApproachIdAndDateRange(int approachID, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public List<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetLastAggregationDate()
        {
            throw new NotImplementedException();
        }

        public List<PhaseLeftTurnGapAggregation> GetPhaseLeftTurnGapAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public int GetSummedGapsBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate, int gapCountColumn)
        {
            throw new NotImplementedException();
        }

        public void Remove(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation)
        {
            throw new NotImplementedException();
        }

        public void Update(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
