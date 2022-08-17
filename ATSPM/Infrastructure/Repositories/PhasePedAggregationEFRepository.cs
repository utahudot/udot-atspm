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
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<PhasePedAggregation> GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        PhasePedAggregation IPhasePedAggregationRepository.Add(PhasePedAggregation pedAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
