using System;

namespace Reports.Business.Common
{
    public class CycleDataPoint
    {
        public CycleDataPoint(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; set; }
        public double Seconds { get; set; }
    }
}
