﻿using System;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
{
    public class AverageSpeeds
    {
        public AverageSpeeds(DateTime startTime, int averageMph)
        {
            StartTime = startTime;
            AverageMph = averageMph;
        }

        public DateTime StartTime { get; set; }
        public int AverageMph { get; set; }

    }
}