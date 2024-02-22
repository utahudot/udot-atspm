using ATSPM.ReportApi.Business.Common;
using System;

namespace ATSPM.ReportApi.Business.PreemptService
{
    public class PreemptPlan : Plan
    {
        public PreemptPlan(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            int preemptCount) : base(planNumber, startTime, endTime)
        {
            PreemptCount = preemptCount;
        }

        public int PreemptCount { get; internal set; }
    }
}