using System;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class FailLine
    {
        public FailLine(DateTime startTime)
        {
            StartTime = startTime;
        }

        public DateTime StartTime { get; internal set; }
    }
}