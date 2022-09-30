using System;

namespace ATSPM.Application.Reports.ViewModels.PreemptDetail
{
    public class InputOff
    {
        public InputOff(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; set; }
        public double Seconds { get; set; }
    }
}