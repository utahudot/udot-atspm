using System;

namespace ATSPM.Application.Reports.Business.LeftTurnGapAnalysis
{
    public class PercentTurnableSeries
    {
        public PercentTurnableSeries(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; internal set; }
        public double Seconds { get; internal set; }
    }
}