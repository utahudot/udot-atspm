using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.PreemptService
{
    /// <summary>
    /// Preempt Service chart
    /// </summary>
    public class PreemptServiceResult : SignalResult
    {
        public PreemptServiceResult(
            string locationId,
            DateTime start,
            DateTime end,
            ICollection<PreemptPlan> plans,
            ICollection<DataPointForInt> preemptServiceEvents) : base(locationId, start, end)
        {
            Plans = plans;
            PreemptServiceEvents = preemptServiceEvents;
        }
        public ICollection<PreemptPlan> Plans { get; }
        public ICollection<DataPointForInt> PreemptServiceEvents { get; }
    }
}