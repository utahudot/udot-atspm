using System;

namespace ATSPM.Application.Reports.ViewModels.LeftTurnGapAnalysis
{
    public class Gap1Count
    {
        public Gap1Count(DateTime startTime, int gaps)
        {
            StartTime = startTime;
            Gaps = gaps;
        }

        public DateTime StartTime { get; internal set; }
        public int Gaps { get; internal set; }
    }
}