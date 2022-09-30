using System;

namespace ATSPM.Application.Reports.ViewModels.PedDelay
{
    public class PedestrianDelay
    {
        public PedestrianDelay(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; internal set; }
        public double Seconds { get; internal set; }

    }
}