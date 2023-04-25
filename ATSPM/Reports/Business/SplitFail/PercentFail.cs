using System;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class PercentFail
    {
        public PercentFail(DateTime startTime, double percent)
        {
            StartTime = startTime;
            Percent = percent;
        }

        public DateTime StartTime { get; internal set; }
        public double Percent { get; internal set; }
    }
}