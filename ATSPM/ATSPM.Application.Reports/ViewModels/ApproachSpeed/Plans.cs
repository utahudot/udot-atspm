
using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachSpeed
{
    public class SpeedPlan
    {
        public SpeedPlan(
            DateTime startTime,
            DateTime endTime,
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
            StartTime = startTime;
            EndTime = endTime;
        }

        public int AverageSpeed { get; set; }
        public int StandardDeviation { get; set; }
        public int EightyFifthPercentile { get; set; }
        public int FifteenthPercentile { get; set; }
        public string PlanNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }
}