﻿using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.LeftTurnGapReport
{
    public class SplitFailResult
    {
        public double SplitFailPercent { get; set; }
        public int CyclesWithSplitFails { get; set; }
        public Dictionary<DateTime, double> PercentCyclesWithSplitFailList { get; internal set; }
        public string Direction { get; set; }
    }
}