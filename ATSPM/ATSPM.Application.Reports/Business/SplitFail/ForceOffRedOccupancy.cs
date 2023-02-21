using System;

namespace ATSPM.Application.Reports.Business.SplitFail
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