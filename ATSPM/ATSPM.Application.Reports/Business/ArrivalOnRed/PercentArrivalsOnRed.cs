using System;

namespace ATSPM.Application.Reports.Business.ArrivalOnRed
{
    public class PercentArrivalsOnRed
    {
        public PercentArrivalsOnRed(DateTime startTime, double volume)
        {
            StartTime = startTime;
            Volume = volume;
        }

        public DateTime StartTime { get; set; }
        public double Volume { get; set; }
    }
}