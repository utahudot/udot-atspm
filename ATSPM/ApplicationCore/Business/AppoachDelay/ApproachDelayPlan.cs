using ATSPM.Application.Business.Common;
using System;

namespace ATSPM.Application.Business.AppoachDelay
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
            PlanNumber = planNumber;
            PlanDescription = planDescription;
        }

        public double AverageDelay { get; }
        public double TotalDelay { get; }
        public string PlanDescription { get; }
    }
}
