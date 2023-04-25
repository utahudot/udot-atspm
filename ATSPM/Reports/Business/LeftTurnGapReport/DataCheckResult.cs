namespace ATSPM.Application.Reports.Business.LeftTurnGapReport
{
    public class DataCheckResult
    {
        public bool LeftTurnVolumeOk { get; set; }
        public bool GapOutOk { get; set; }
        public bool PedCycleOk { get; set; }
        public string ApproachDescriptions { get; set; }
        public bool InsufficientDetectorEventCount { get; set; }
        public bool InsufficientCycleAggregation { get; set; }
        public bool InsufficientPhaseTermination { get; set; }
        public bool InsufficientPedAggregations { get; set; }
        public bool InsufficientSplitFailAggregations { get; internal set; }
        public bool InsufficientLeftTurnGapAggregations { get; internal set; }
    }
}
