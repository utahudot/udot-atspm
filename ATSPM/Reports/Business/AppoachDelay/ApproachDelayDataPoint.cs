using System;

namespace ATSPM.Application.Reports.Business.AppoachDelay
{
    public class ApproachDelayDataPoint
    {
        public ApproachDelayDataPoint(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; set; }
        public double Seconds { get; set; }
    }
}
