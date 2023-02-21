using System;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class AverageRor
    {
        public AverageRor(DateTime startTime, double average)
        {
            StartTime = startTime;
            Average = average;
        }

        public DateTime StartTime { get; internal set; }
        public double Average { get; internal set; }
    }
}