using System;

namespace ATSPM.Application.Reports.Business.Common
{
    public class PlanSplitMonitorDTO
    {

        public string PlanNumber { get; internal set; }
        public string PlanDescription { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public int HighCycleCount { get; set; }
        public double PercentSkips { get; set; }
        public double PercentGapOuts { get; set; }
        public double PercentMaxOuts { get; set; }
        public double PercentForceOffs { get; set; }
        public double AverageSplit { get; set; }
        public double PercentileSplit { get; set; }
    }
}