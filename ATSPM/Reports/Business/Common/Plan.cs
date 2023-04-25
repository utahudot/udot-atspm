using System;

namespace ATSPM.Application.Reports.Business.Common
{
    public class Plan
    {
        public Plan(string planNumber, DateTime startTime, DateTime endTime)
        {
            PlanNumber = planNumber;
            StartTime = startTime;
            EndTime = endTime;
        }

        public string PlanNumber { get; internal set; }
        public DateTime StartTime { get; internal set; }
        public DateTime EndTime { get; internal set; }
    }
}
