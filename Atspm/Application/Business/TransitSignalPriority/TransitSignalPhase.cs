namespace Utah.Udot.Atspm.Business.TransitSignalPriority
{
    public class TransitSignalPhase
    {
        public int PhaseNumber { get; set; }
        public double MinGreen { get; set; }
        public double Yellow { get; set; }
        public double RedClearance { get; set; }
        public double MinTime { get; set; }
        public double ProgrammedSplit { get; set; }
        public double PercentileSplit85th { get; set; }
        public double PercentileSplit50th { get; set; }
        public double AverageSplit { get; set; }
        public double PercentMaxOutsForceOffs { get; set; }
        public double PercentGapOuts { get; set; }
        public double PercentSkips { get; set; }
        public double? RecommendedTSPMax { get; set; }
        public double SkipsGreaterThan70TSPMax { get; set; }
        public double ForceOffsGreaterThan40TSPMax { get; set; }
        public double ForceOffsGreaterThan60TSPMax { get; set; }
        public double ForceOffsGreaterThan80TSPMax { get; set; }
        public int MaxReduction { get; set; }
        public int MaxExtension { get; set; }
        public int PriorityMin { get; set; }
        public int PriorityMax { get; set; }
        public string Notes { get; set; }

    }
}