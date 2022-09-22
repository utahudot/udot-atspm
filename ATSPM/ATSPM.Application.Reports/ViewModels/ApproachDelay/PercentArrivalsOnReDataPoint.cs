

using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachDelay
{
    public class PercentArrivalsOnReDataPoint
    {
        public PercentArrivalsOnReDataPoint(DateTime startTime, double percent)
        {
            this.StartTime = startTime;
            this.Percent = percent;
        }

        public DateTime StartTime { get; set; }
        public double Percent { get; set; }
    }
}
