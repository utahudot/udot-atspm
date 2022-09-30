
using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachSpeed
{
    public class Plans
    {

        public int AverageSpeed { get; set; }
        public int StandardDeviation { get; set; }
        public int EightyFifthPercentile { get; set; }
        public int FifteenthPercentile { get; set; }
        public string PlanNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }
}