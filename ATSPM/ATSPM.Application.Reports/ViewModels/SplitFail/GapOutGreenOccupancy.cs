using System;

namespace ATSPM.Application.Reports.ViewModels.SplitFail
{
    public class GapOutGreenOccupancy
    {
        public GapOutGreenOccupancy(DateTime startTime, double percent)
        {
            StartTime = startTime;
            Percent = percent;
        }

        public DateTime StartTime { get; internal set; }
        public double Percent { get; internal set; }
    }
}