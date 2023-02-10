using System;

namespace ATSPM.Application.Reports.Business.PreempDetail
{
    public class DwellTime
    {
        public DwellTime(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; set; }
        public double Seconds { get; set; }
    }
}