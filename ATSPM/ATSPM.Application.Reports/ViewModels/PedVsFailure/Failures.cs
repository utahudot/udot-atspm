using System;

namespace ATSPM.Application.Reports.ViewModels.PedVsFailure
{
    public class Failures
    {
        public Failures(DateTime startTime, double percent)
        {
            StartTime = startTime;
            Percent = percent;
        }

        public DateTime StartTime { get; internal set; }
        public double Percent { get; internal set; }
    }
}