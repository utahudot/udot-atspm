using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class VolumeResult
    {
        public int OpposingLanes { get; set; }
        public bool CrossProductReview { get; set; }
        public bool DecisionBoundariesReview { get; set; }
        public double LeftTurnVolume { get; set; }
        public double OpposingThroughVolume { get; set; }
        public double CrossProductValue { get; set; }
        public double CalculatedVolumeBoundary { get; set; }
        public Dictionary<DateTime, double> DemandList { get; set; }
        public string Direction { get; internal set; }
        public string OpposingDirection { get; internal set; }
    }
}


