﻿using ATSPM.ReportApi.Business.Common;
using System;

namespace ATSPM.ReportApi.Business.ArrivalOnRed
{
    public class ArrivalOnRedPlan : Plan
    {
        public ArrivalOnRedPlan(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            double percentArrivalOnRed,
            double percentRedTime) : base(planNumber, startTime, endTime)
        {
            PercentArrivalOnRed = percentArrivalOnRed;
            PercentRedTime = percentRedTime;
        }

        public double PercentArrivalOnRed { get; set; }
        public double PercentRedTime { get; set; }

    }
}