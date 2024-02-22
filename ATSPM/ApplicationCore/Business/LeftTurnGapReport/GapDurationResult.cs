using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class GapDurationResult
    {
        public double GapDurationPercent { get; set; }
        public double Capacity { get; set; }
        public double Demand { get; set; }
        public Dictionary<DateTime, double> AcceptableGaps { get; set; }
        public string Direction { get; set; }
        public string OpposingDirection { get; internal set; }
    }
}
