using System;

namespace ATSPM.Application.Reports.Business.PedDelay
{
    public class CycleLengths
    {
        public CycleLengths(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; internal set; }
        public double Seconds { get; internal set; }

    }
}