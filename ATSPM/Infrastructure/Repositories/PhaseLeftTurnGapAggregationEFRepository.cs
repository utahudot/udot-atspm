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
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<PhaseLeftTurnGapAggregation> GetPhaseLeftTurnGapAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public int GetSummedGapsBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate, int gapCountColumn)
        {
            throw new NotImplementedException();
        }

        PhaseLeftTurnGapAggregation IPhaseLeftTurnGapAggregationRepository.Add(PhaseLeftTurnGapAggregation phaseLeftTurnGapAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
