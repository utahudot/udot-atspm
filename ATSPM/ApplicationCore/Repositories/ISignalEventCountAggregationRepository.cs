using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    public interface ISignalEventCountAggregationRepository:IAggregationRepositoryBase
    {
        int GetSignalEventCountSumAggregationBySignalIdAndDateRange(string signalId, DateTime start, DateTime end);

        List<SignalEventCountAggregation> GetSignalEventCountAggregationBySignalIdAndDateRange(string signalId, DateTime start,
            DateTime end );
    }
}