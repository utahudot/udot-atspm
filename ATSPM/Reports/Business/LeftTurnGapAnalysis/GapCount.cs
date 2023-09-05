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

        public GapCount()
        {
        }

        public DateTime StartTime { get; set; }
        public int Gaps { get; set; }
    }
}