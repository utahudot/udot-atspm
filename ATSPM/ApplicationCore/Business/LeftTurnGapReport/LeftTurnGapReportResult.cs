using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class LeftTurnGapReportResult
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ApproachDescription { get; set; }
        public string SignalId { get; set; }
        public string Location { get; set; }
        public bool Get24HourPeriod { get; set; }
        public string PhaseType { get; set; }
        public string SignalType { get; set; }
        public int? SpeedLimit { get; set; }
        public string PeakPeriodDescription { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int CyclesWithSplitFailNum { get; set; }
        public double CyclesWithSplitFailPercent { get; set; }
        public int CyclesWithPedCallNum { get; set; }
        public double CyclesWithPedCallPercent { get; set; }
        public double CrossProductValue { get; set; }
        public double CalculatedVolumeBoundary { get; set; }
        public bool? GapDurationConsiderForStudy { get; set; }
        public bool? SplitFailsConsiderForStudy { get; set; }
        public bool? PedActuationsConsiderForStudy { get; set; }
        public bool? VolumesConsiderForStudy { get; set; }
        public double Capacity { get; set; }
        public double Demand { get; set; }
        public double GapOutPercent { get; set; }
        public int OpposingLanes { get; set; }
        public bool CrossProductReview { get; set; }
        public bool DecisionBoundariesReview { get; set; }
        public double LeftTurnVolume { get; set; }
        public double OpposingThroughVolume { get; set; }
        public bool? CrossProductConsiderForStudy { get; set; }
        public Dictionary<DateTime, double> AcceptableGapList { get; set; }
        public Dictionary<DateTime, double> PercentCyclesWithPedsList { get; set; }
        public Dictionary<DateTime, double> DemandList { get; set; }
        public Dictionary<DateTime, double> PercentCyclesWithSplitFailList { get; set; }
        public string Direction { get; set; }
        public string OpposingDirection { get; set; }
    }
}
