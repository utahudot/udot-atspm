using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PreemptServiceRequest
{
    /// <summary>
    /// Preempt Service Request chart
    /// </summary>
    public class PreemptServiceRequestResult : SignalResult
    {
        public PreemptServiceRequestResult(string chartName,
            string signalId,
            DateTime start,
            DateTime end,
            IReadOnlyList<Plan> plans,
            IReadOnlyList<DataPointForInt> preemptRequests) : base(signalId, start, end)
        {
            Plans = plans;
            PreemptRequests = preemptRequests;
        }
        public IReadOnlyList<Plan> Plans { get; internal set; }
        public IReadOnlyList<DataPointForInt> PreemptRequests { get; internal set; }
    }
}