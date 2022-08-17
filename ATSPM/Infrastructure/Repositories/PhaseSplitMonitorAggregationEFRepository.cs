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
    public class PhaseSplitMonitorAggregationEFRepository : ATSPMRepositoryEFBase<PhaseSplitMonitorAggregation>, IPhaseSplitMonitorAggregationRepository
    {
        public PhaseSplitMonitorAggregationEFRepository(DbContext db, ILogger<PhaseSplitMonitorAggregationEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<PhaseSplitMonitorAggregation> GetSplitMonitorAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        PhaseSplitMonitorAggregation IPhaseSplitMonitorAggregationRepository.Add(PhaseSplitMonitorAggregation splitMonitorAggregation)
        {
            throw new NotImplementedException();
        }
    }
}
