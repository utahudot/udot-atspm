using System;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
{
    public class FifteenthSpeeds
    {
        public FifteenthSpeeds(DateTime startTime, double fifteenthMph)
        {
            StartTime = startTime;
            FifteenthMph = fifteenthMph;
        }

        public DateTime StartTime { get; set; }
        public double FifteenthMph { get; set; }

    }
}