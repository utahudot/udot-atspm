using System;

namespace ATSPM.Application.Reports.ViewModels.SplitFail
{
    public class ForceOffRedOccupancy
    {
        public ForceOffRedOccupancy(DateTime startTime, double percent)
        {
            StartTime = startTime;
            Percent = percent;
        }

        public DateTime StartTime { get; internal set; }
        public double Percent { get; internal set; }
    }
}