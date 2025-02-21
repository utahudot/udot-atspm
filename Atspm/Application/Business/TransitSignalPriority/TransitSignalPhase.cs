namespace Utah.Udot.Atspm.Business.TransitSignalPriority
{
    public class TransitSignalPhase
    {
        public int PhaseNumber { get; set; }
        public double MinTime { get; set; }
        public double ProgrammedSplit { get; set; }
        public double PercentileSplit85th { get; set; }
        public double PercentileSplit50th { get; set; }
        public double AverageSplit { get; set; }
        public double PercentMaxOutsForceOffs { get; set; }
        public double PercentGapOuts { get; set; }
        public double PercentSkips { get; set; }
    }
}