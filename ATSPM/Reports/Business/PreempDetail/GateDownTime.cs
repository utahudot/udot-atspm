﻿using System;

namespace ATSPM.Application.Reports.Business.PreempDetail
{
    public class GateDownTime
    {
        public GateDownTime(DateTime startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public DateTime StartTime { get; set; }
        public double Seconds { get; set; }
    }
}