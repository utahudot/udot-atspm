using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachDelay
{
    public class ApproachDelayPlan : Plan
    {
        public ApproachDelayPlan(
            double averageDelay,
            double totalDelay,
            DateTime start,
            DateTime end,
            string planNumber,
            string planDescription) : base(planNumber, start, end)
        {
            AverageDelay = averageDelay;
            TotalDelay = totalDelay;
            Start = start;
            End = end;
            PlanNumber = planNumber;
            PlanDescription = planDescription;
        }

        public double AverageDelay { get; }
        public double TotalDelay { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public string PlanNumber { get; }
        public string PlanDescription { get; }
    }
}
