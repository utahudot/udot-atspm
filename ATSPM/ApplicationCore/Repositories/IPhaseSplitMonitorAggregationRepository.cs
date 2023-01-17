using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    public interface IPhaseSplitMonitorAggregationRepository:IAggregationRepositoryBase
    {

        PhaseSplitMonitorAggregation Add(PhaseSplitMonitorAggregation splitMonitorAggregation);
        void Update(PhaseSplitMonitorAggregation splitMonitorAggregation);
        void Remove(PhaseSplitMonitorAggregation splitMonitorAggregation);
        List<PhaseSplitMonitorAggregation> GetSplitMonitorAggregationBySignalIdPhaseNumberAndDateRange(string signalId, int phaseNumber, DateTime startDate, DateTime endDate);
        List<int> GetAvailablePhaseNumbers(Signal signal, DateTime startDate, DateTime endDate);
    }
}