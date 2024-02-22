using ATSPM.Application.Business.Common;
using System;

namespace ATSPM.Application.Business.WaitTime
{
    public class PlanWaitTime : Plan
    {
        public PlanWaitTime(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            double averageWaitTime,
            double maxWaitTime) : base(planNumber, startTime, endTime)
        {
            AverageWaitTime = averageWaitTime;
            MaxWaitTime = maxWaitTime;
        }

        public double AverageWaitTime { get; set; }
        public double MaxWaitTime { get; set; }

    }
}