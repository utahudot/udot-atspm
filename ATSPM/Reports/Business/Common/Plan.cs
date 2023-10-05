using System;

namespace ATSPM.Application.Reports.Business.Common
{
    public class Plan
    {
        public Plan(string planNumber, DateTime startTime, DateTime endTime)
        {
            PlanNumber = planNumber;
            Start = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
            End = DateTime.SpecifyKind(endTime, DateTimeKind.Unspecified);
        }

        public string PlanNumber { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
    }
}
