using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.PreemptService
{
    /// <summary>
    /// Preempt Service chart
    /// </summary>
    public class PreemptServiceResult : LocationResult
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