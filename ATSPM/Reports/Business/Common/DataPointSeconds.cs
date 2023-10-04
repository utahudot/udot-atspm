using System;

namespace Reports.Business.Common
{
    public class DataPointSeconds
    {
        public DataPointSeconds(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; set; }
        public double Seconds { get; set; }
    }
}
