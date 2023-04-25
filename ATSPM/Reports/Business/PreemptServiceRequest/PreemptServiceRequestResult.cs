using ATSPM.Application.Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PreemptServiceRequest
{
    /// <summary>
    /// Preempt Service Request chart
    /// </summary>
    public class PreemptServiceRequestResult
    {
        public PreemptServiceRequestResult(string chartName,
            string signalId,
            DateTime start,
            DateTime end,
            IReadOnlyList<Plan> plans,
            IReadOnlyList<PreemptRequest> preemptRequests)
        {
            ChartName = chartName;
            SignalId = signalId;
            Start = start;
            End = end;
            Plans = plans;
            PreemptRequests = preemptRequests;
        }

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public IReadOnlyList<Plan> Plans { get; internal set; }
        public IReadOnlyList<PreemptRequest> PreemptRequests { get; internal set; }
    }
}