using ATSPM.Application.Business.Common;
using System;

namespace ATSPM.Application.Business.ApproachSpeed
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
        }

        public int? AverageSpeed { get; set; }
        public int? StandardDeviation { get; set; }
        public int? EightyFifthPercentile { get; set; }
        public int? FifteenthPercentile { get; set; }

    }
}