using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PreemptService
{
    /// <summary>
    /// Preempt Service chart
    /// </summary>
    public class PreemptServiceResult:SignalResult
    {
        public PreemptServiceResult(
            string signalId,
            DateTime start,
            DateTime end,
            ICollection<PreemptPlan> plans,
            ICollection<PreemptServiceEvent> preemptServiceEvents):base(signalId, start, end)
        {
            Plans = plans;
            PreemptServiceEvents = preemptServiceEvents;
        }
        public ICollection<PreemptPlan> Plans { get; internal set; }
        public ICollection<PreemptServiceEvent> PreemptServiceEvents { get; internal set; }
    }
}