using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    public interface IPhaseLeftTurnGapAggregationRepository:IAggregationRepositoryBase
    {
        PhaseLeftTurnGapAggregation Add(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation);
        void Update(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation);
        void Remove(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation);
        List<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate);
        List<PhaseLeftTurnGapAggregation> GetPhaseLeftTurnGapAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate);
        int GetSummedGapsBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate, int gapCountColumn);
        List<PhaseLeftTurnGapAggregation> GetAggregationByApproachIdAndDateRange(int approachID, DateTime startDate, DateTime endDate);
        bool Exists(string signalId, int phaseNumber, DateTime startDate, DateTime endDate);
    }
}