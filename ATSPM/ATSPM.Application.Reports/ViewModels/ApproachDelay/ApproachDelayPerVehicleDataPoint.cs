﻿using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachDelay
{
    public class ApproachDelayPerVehicleDataPoint
    {
        public ApproachDelayPerVehicleDataPoint(DateTime startTime, double delayPerVehicle)
        {
            StartTime = startTime;
            DelayPerVehicle = delayPerVehicle;
        }

        public DateTime StartTime { get; set; }
        public double DelayPerVehicle { get; set; }
    }
}