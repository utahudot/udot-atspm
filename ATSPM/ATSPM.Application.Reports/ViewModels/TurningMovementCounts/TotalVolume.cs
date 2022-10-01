using System;

namespace ATSPM.Application.Reports.ViewModels.TurningMovementCounts
{
    public partial class TotalVolume
    {
        public TotalVolume(DateTime startTime, int volume)
        {
            StartTime = startTime;
            Volume = volume;
        }

        public DateTime StartTime { get; internal set; }
        public int Volume { get; internal set; }
    }
}