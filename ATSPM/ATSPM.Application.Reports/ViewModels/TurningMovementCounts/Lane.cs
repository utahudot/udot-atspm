using System;

namespace ATSPM.Application.Reports.ViewModels.TurningMovementCounts
{
    public class Lane
    {
        public Lane(DateTime startTime, int volume)
        {
            StartTime = startTime;
            Volume = volume;
        }

        public DateTime StartTime { get; internal set; }
        public int Volume { get; internal set; }
    }
}