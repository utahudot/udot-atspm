using System;

namespace ATSPM.Application.Reports.Business.ArrivalOnRed
{
    public class ArrivalsOnRed
    {
        public ArrivalsOnRed(DateTime startTime, double volume)
        {
            StartTime = startTime;
            Volume = volume;
        }

        public DateTime StartTime { get; set; }
        public double Volume { get; set; }

    }
}