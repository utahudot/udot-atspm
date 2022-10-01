using System;

namespace ATSPM.Application.Reports.ViewModels.TurningMovementCounts
{
    public class ThruRight
    {
        public ThruRight(DateTime startTime, double volume)
        {
            StartTime = startTime;
            Volume = volume;
        }

        public DateTime StartTime { get; internal set; }
        public double Volume { get; internal set; }
    }
}