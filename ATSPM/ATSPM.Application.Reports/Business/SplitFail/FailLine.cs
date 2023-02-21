using System;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class FailLine
    {
        public FailLine(DateTime startTime, int number)
        {
            StartTime = startTime;
            Number = number;
        }

        public DateTime StartTime { get; internal set; }
        public int Number { get; internal set; }
    }
}