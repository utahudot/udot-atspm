using System;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class AverageGor
    {
        public AverageGor(DateTime startTime, double average)
        {
            StartTime = startTime;
            Average = average;
        }

        public DateTime StartTime { get; internal set; }
        public double Average { get; internal set; }
    }
}