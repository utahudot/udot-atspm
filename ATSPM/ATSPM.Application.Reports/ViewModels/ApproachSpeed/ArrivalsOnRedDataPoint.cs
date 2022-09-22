using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachDelay
{
    public class ArrivalsOnRedDataPoint
    {
        public ArrivalsOnRedDataPoint(DateTime startTime, double arrivals)
        {
            StartTime = startTime;
            this.Arrivals = arrivals;
        }

        public DateTime StartTime { get; set; }
        public double Arrivals { get; set; }
    }
}
