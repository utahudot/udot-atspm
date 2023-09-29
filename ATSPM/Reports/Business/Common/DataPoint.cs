using System;

namespace Reports.Business.Common
{
    public class DataPoint
    {
        public DataPoint(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; set; }
        public double Seconds { get; set; }
    }
}
