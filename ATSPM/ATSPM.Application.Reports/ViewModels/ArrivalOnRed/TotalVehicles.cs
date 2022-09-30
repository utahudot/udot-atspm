using System;

namespace ATSPM.Application.Reports.ViewModels.ArrivalOnRed
{
    public class TotalVehicles
    {
        public TotalVehicles(DateTime startTime, double volume)
        {
            StartTime = startTime;
            Volume = volume;
        }

        public DateTime StartTime { get; set; }
        public double Volume { get; set; }

    }
}