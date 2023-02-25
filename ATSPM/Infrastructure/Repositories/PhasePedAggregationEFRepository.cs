using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public class PhasePedAggregationEFRepository : IPhasePedAggregationRepository
    {
        public PhasePedAggregation Add(PhasePedAggregation pedAggregation)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public List<PhaseCycleAggregation> GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(string signalId, int phase, DateTime start, DateTime end)
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

        public List<PhasePedAggregation> GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public void Remove(PhasePedAggregation pedAggregation)
        {
            throw new NotImplementedException();
        }

        public void Update(PhasePedAggregation pedAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
