using System;

namespace ATSPM.Application.Reports.ViewModels.LeftTurnGapAnalysis
{
    public class Gap2Count
    {
        public Gap2Count(DateTime startTime, int gaps)
        {
            StartTime = startTime;
            Gaps = gaps;
        }

        public DateTime StartTime { get; internal set; }
        public int Gaps { get; internal set; }
    }
}