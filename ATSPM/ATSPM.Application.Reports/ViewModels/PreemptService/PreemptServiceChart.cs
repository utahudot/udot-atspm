using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.PreemptService
{
    /// <summary>
    /// Preempt Service chart
    /// </summary>
    public class PreemptServiceChart
    {
        public PreemptServiceChart(
            string chartName,
            string signalId,
            string signalLocation,
            DateTime start,
            DateTime end,
            ICollection<PreemptPlan> plans,
            ICollection<PreemptServiceEvent> preemptServiceEvents)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            Start = start;
            End = end;
            Plans = plans;
            PreemptServiceEvents = preemptServiceEvents;
        }

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public string SignalLocation { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public ICollection<PreemptPlan> Plans { get; internal set; }
        public ICollection<PreemptServiceEvent> PreemptServiceEvents { get; internal set; }
    }
}