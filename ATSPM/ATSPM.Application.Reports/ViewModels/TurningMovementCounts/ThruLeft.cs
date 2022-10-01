using System;

namespace ATSPM.Application.Reports.ViewModels.TurningMovementCounts
{
    public class ThruLeft
    {
        public ThruLeft(DateTime startTime, int volume)
        {
            StartTime = startTime;
            Volume = volume;
        }

        public DateTime StartTime { get; internal set; }
        public int Volume { get; internal set; }
    }
}