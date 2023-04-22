using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PreemptService
{
    /// <summary>
    /// Preempt Service chart
    /// </summary>
    public class PreemptServiceResult
    {
        public PreemptServiceResult(
            string chartName,
            string signalId,
            DateTime start,
            DateTime end,
            ICollection<PreemptPlan> plans,
            ICollection<PreemptServiceEvent> preemptServiceEvents)
        {
            ChartName = chartName;
            SignalId = signalId;
            Start = start;
            End = end;
            Plans = plans;
            PreemptServiceEvents = preemptServiceEvents;
        }

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public ICollection<PreemptPlan> Plans { get; internal set; }
        public ICollection<PreemptServiceEvent> PreemptServiceEvents { get; internal set; }
    }
}