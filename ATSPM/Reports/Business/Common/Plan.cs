using System;

namespace ATSPM.Application.Reports.Business.Common
{
    public class Plan
    {
        public Plan(string planNumber, DateTime startTime, DateTime endTime)
        {
            PlanNumber = planNumber;
            Start = startTime;
            EndTime = endTime;
        }

        public string PlanNumber { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime EndTime { get; internal set; }
    }
}
