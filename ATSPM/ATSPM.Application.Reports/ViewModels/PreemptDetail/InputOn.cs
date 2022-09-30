using System;

namespace ATSPM.Application.Reports.ViewModels.PreemptDetail
{
    public class InputOn
    {
        public InputOn(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; set; }
        public double Seconds { get; set; }
    }
}