using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.Common
{
    public class PlanSplitMonitorData : Plan
    {
        public PlanSplitMonitorData(DateTime start, DateTime end, string planNumber) : base(planNumber, start, end)
        {
            Splits = new SortedDictionary<int, int>();
        }

        public SortedDictionary<int, int> Splits { get; set; }
        public int CycleLength { get; set; }
        public int OffsetLength { get; set; }
        public int HighCycleCount { get; set; }
        public double PercentSkips { get; set; }
        public double PercentGapOuts { get; set; }
        public double PercentMaxOuts { get; set; }
        public double PercentForceOffs { get; set; }
        public double AverageSplit { get; set; }
        public double PercentileSplit { get; set; }
        public double MinTime { get; set; }
        public double ProgrammedSplit { get; set; }
        public double PercentileSplit85th { get; set; }
        public double PercentileSplit50th { get; set; }
    }
}