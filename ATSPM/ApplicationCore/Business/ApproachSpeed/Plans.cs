using ATSPM.ReportApi.Business.Common;
using System;

namespace ATSPM.ReportApi.Business.ApproachSpeed
{
    public class SpeedPlan : Plan
    {
        public SpeedPlan(
            DateTime start,
            DateTime end,
            string planNumber,
            int? averageSpeed,
            int? standardDeviation,
            int? eightyFifthPercentile,
            int? fifteenthPercentile) : base(planNumber, start, end)
        {
            AverageSpeed = averageSpeed;
            StandardDeviation = standardDeviation;
            EightyFifthPercentile = eightyFifthPercentile;
            FifteenthPercentile = fifteenthPercentile;
            //PlanNumber = planNumber;
            //Start = start;
            //End = end;
        }

        public int? AverageSpeed { get; set; }
        public int? StandardDeviation { get; set; }
        public int? EightyFifthPercentile { get; set; }
        public int? FifteenthPercentile { get; set; }
        //public string PlanNumber { get; set; }
        //public DateTime Start { get; set; }
        //public DateTime End { get; set; }

    }
}