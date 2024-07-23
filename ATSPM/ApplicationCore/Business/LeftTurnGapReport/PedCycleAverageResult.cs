using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class PedCycleAverageResult
    {
        public double PedCycleAverage { get; set; }
        public Dictionary<DateTime, double> PedCycleAverageList { get; set; }
    }
}