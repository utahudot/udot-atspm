using System;

namespace ATSPM.Application.Reports.ViewModels.WaitTime
{
    public class GapOut
    {
        public GapOut(DateTime startTime, double waitTime)
        {
            StartTime = startTime;
            WaitTime = waitTime;
        }

        public DateTime StartTime { get; internal set; }
        public double WaitTime { get; internal set; }
    }
}