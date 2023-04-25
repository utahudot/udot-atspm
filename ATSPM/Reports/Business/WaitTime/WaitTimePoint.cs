using System;

namespace ATSPM.Application.Reports.Business.WaitTime
{
    public class WaitTimePoint
    {
        public WaitTimePoint(DateTime startTime, double waitTime)
        {
            StartTime = startTime;
            WaitTime = waitTime;
        }

        public DateTime StartTime { get; internal set; }
        public double WaitTime { get; internal set; }
    }
}