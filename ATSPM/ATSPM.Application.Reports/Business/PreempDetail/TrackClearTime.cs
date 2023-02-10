using System;

namespace ATSPM.Application.Reports.Business.PreempDetail
{
    public class TrackClearTime
    {
        public TrackClearTime(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; set; }
        public double Seconds { get; set; }
    }
}