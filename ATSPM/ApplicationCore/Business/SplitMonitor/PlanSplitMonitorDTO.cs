using System;

namespace ATSPM.Application.Business.SplitMonitor
{
    public class PlanSplitMonitorDTO
    {

        public string PlanNumber { get; internal set; }
        public string PlanDescription { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
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