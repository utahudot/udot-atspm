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
    public class SignalEventCountAggregationEFRepository : ATSPMRepositoryEFBase<SignalEventCountAggregation>, ISignalEventCountAggregationRepository
    {
        public SignalEventCountAggregationEFRepository(DbContext db, ILogger<SignalEventCountAggregationEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<SignalEventCountAggregation> GetSignalEventCountAggregationBySignalIdAndDateRange(string signalId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public int GetSignalEventCountSumAggregationBySignalIdAndDateRange(string signalId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }
    }
}
