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
            string signalLocation,
            DateTime start,
            DateTime end,
            ICollection<Plan> plans,
            ICollection<PreemptRequest> preemptRequests)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            Start = start;
            End = end;
            Plans = plans;
            PreemptRequests = preemptRequests;
        }

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public string SignalLocation { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public ICollection<Plan> Plans { get; internal set; }
        public ICollection<PreemptRequest> PreemptRequests { get; internal set; }
    }
}