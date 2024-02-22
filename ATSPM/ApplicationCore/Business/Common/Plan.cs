using ATSPM.Application.Analysis.Plans;
using System;

namespace ATSPM.Application.Business.Common
{
    public class Plan
    {
        public Plan(string planNumber, DateTime startTime, DateTime endTime)
        {
            PlanNumber = planNumber;
            Start = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
            End = DateTime.SpecifyKind(endTime, DateTimeKind.Unspecified);
            PlanDescription = getPlanDescription();
        }

        private string getPlanDescription()
        {
            var planDescription = "Unknown";
            switch (PlanNumber)
            {
                case "254":
                    planDescription = "Free";
                    break;
                case "255":
                    planDescription = "Flash";
                    break;
                case "0":
                    planDescription = "Unknown";
                    break;
                default:
                    planDescription = "Plan " + PlanNumber;

                    break;
            }

            return planDescription;
        }

        public string PlanNumber { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public string PlanDescription { get; }
    }
}
