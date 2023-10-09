using System;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
{
    public class SpeedPlan
    {
        public SpeedPlan(
            DateTime start,
            DateTime end,
            string planNumber,
            int averageSpeed,
            int standardDeviation,
            int eightyFifthPercentile,
            int fifteenthPercentile)
        {
            AverageSpeed = averageSpeed;
            StandardDeviation = standardDeviation;
            EightyFifthPercentile = eightyFifthPercentile;
            FifteenthPercentile = fifteenthPercentile;
            PlanNumber = planNumber;
            Start = start;
            End = end;
        }

        public int AverageSpeed { get; set; }
        public int StandardDeviation { get; set; }
        public int EightyFifthPercentile { get; set; }
        public int FifteenthPercentile { get; set; }
        public string PlanNumber { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

    }
}