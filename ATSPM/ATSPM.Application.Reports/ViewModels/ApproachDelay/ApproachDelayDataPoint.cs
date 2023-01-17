using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachDelay
{
    public class ApproachDelayDataPoint
    {
        public ApproachDelayDataPoint(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            this.Seconds = seconds;
        }

        public DateTime StartTime { get; set; }
        public double Seconds { get; set; }
    }
}
