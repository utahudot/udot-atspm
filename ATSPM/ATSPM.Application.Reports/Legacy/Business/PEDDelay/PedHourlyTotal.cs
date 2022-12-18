using System;

namespace Legacy.Common.Business.PEDDelay
{
    public class PedHourlyTotal
    {
        public PedHourlyTotal(DateTime hour, double delaySeconds)
        {
            Hour = hour;
            Delay = delaySeconds;
        }

        public DateTime Hour { get; }

        public double Delay { get; }
    }
}