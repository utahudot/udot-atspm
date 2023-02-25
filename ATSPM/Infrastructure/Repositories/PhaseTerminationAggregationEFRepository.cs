using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public class PhaseTerminationAggregationEFRepository : IPhaseTerminationAggregationRepository
    {
        public PhaseTerminationAggregation Add(PhaseTerminationAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
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

        public List<PhaseTerminationAggregation> GetPhaseTerminationsAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public void Remove(PhaseTerminationAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }

        public void Update(PhaseTerminationAggregation priorityAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
