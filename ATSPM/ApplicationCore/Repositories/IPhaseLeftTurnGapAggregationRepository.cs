using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IPhaseLeftTurnGapAggregationRepository : IAsyncRepository<PhaseLeftTurnGapAggregation>
    {
        [Obsolete("Use Add in the BaseClass")]
        PhaseLeftTurnGapAggregation Add(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation);
        [Obsolete("Use Update in the BaseClass")]
        void Update(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation);
        IReadOnlyCollection<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate);
        IReadOnlyCollection<PhaseLeftTurnGapAggregation> GetPhaseLeftTurnGapAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate);
        int GetSummedGapsBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate, int gapCountColumn);
        IReadOnlyCollection<PhaseLeftTurnGapAggregation> GetAggregationByApproachIdAndDateRange(int approachID, DateTime startDate, DateTime endDate);
    }
}
