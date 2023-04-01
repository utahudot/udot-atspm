using System;

namespace ATSPM.Application.Reports.Business.YellowRedActivations
{
    public class YellowRedActivationEvent
    {
        public YellowRedActivationEvent(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; internal set; }
        public double Seconds { get; internal set; }
    }
}