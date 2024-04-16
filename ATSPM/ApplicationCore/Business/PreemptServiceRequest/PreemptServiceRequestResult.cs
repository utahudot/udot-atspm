using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.PreemptServiceRequest
{
    /// <summary>
    /// Preempt Service Request chart
    /// </summary>
    public class PreemptServiceRequestResult : LocationResult
    {
        public PreemptServiceRequestResult(string chartName,
            string locationId,
            DateTime start,
            DateTime end,
            IReadOnlyList<Plan> plans,
            IReadOnlyList<DataPointForInt> preemptRequests) : base(locationId, start, end)
        {
            Plans = plans;
            PreemptRequests = preemptRequests;
        }
        public IReadOnlyList<Plan> Plans { get; internal set; }
        public IReadOnlyList<DataPointForInt> PreemptRequests { get; internal set; }
    }
}