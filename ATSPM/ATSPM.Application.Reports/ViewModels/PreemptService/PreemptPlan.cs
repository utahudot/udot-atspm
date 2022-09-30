using System;

namespace ATSPM.Application.Reports.ViewModels.PreemptService
{
    public class PreemptPlan : Plan
    {
        public PreemptPlan(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            object preemptCount) : base(planNumber, startTime, endTime)
        {
            PreemptCount = preemptCount;
        }

        public object PreemptCount { get; internal set; }
    }
}