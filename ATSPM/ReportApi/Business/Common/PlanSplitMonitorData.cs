﻿using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.Common
{
    public class PlanSplitMonitorData : Plan
    {
        public PlanSplitMonitorData(DateTime start, DateTime end, string planNumber) : base(planNumber, start, end)
        {
            Splits = new SortedDictionary<int, int>();
        }

        public SortedDictionary<int, int> Splits { get; set; }
        public int CycleLength { get; set; }
        public int OffsetLength { get; set; }
        public int HighCycleCount { get; set; }
        public double PercentSkips { get; set; }
        public double PercentGapOuts { get; set; }
        public double PercentMaxOuts { get; set; }
        public double PercentForceOffs { get; set; }
        public double AverageSplit { get; set; }
        public double PercentileSplit { get; set; }
    }
}