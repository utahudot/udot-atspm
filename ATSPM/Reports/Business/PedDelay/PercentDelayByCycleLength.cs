using System;

namespace ATSPM.Application.Reports.Business.PedDelay
{
    public class PercentDelayByCycleLength
    {
        public PercentDelayByCycleLength(DateTime startTime, double percent)
        {
            StartTime = startTime;
            Percent = percent;
        }

        public DateTime StartTime { get; internal set; }
        public double Percent { get; internal set; }
    }
}