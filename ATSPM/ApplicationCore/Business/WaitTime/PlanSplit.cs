using System;

namespace ATSPM.ReportApi.Business.WaitTime
{
    public class PlanSplit
    {
        public PlanSplit(DateTime start, DateTime end, int phaseNumber, int split)
        {
            Start = start;
            End = end;
            PhaseNumber = phaseNumber;
            Split = split;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int PhaseNumber { get; set; }
        public int Split { get; set; }
    }
}