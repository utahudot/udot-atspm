using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class CycleAverageResult
    {
        public double CycleAverage { get; set; }
        public Dictionary<DateTime, double> CycleAverageList { get; set; }
    }
}