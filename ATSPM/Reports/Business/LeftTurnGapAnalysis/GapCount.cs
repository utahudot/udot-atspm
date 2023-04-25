using System;

namespace ATSPM.Application.Reports.Business.LeftTurnGapAnalysis
{
    public class GapCount
    {
        public GapCount(DateTime startTime, int gaps)
        {
            StartTime = startTime;
            Gaps = gaps;
        }

        public DateTime StartTime { get; internal set; }
        public int Gaps { get; internal set; }
    }
}