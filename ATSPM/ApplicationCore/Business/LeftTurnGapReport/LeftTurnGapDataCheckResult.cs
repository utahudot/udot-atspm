using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class LeftTurnGapDataCheckResult : ApproachResult
    {
        public bool LeftTurnVolumeOk { get; set; }
        public bool GapOutOk { get; set; }
        public bool PedCycleOk { get; set; }
        public bool InsufficientDetectorEventCount { get; set; }
        public bool InsufficientCycleAggregation { get; set; }
        public bool InsufficientPhaseTermination { get; set; }
        public bool InsufficientPedAggregations { get; set; }
        public bool InsufficientSplitFailAggregations { get; set; }
        public bool InsufficientLeftTurnGapAggregations { get; set; }
    }
}
