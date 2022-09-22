using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachDelay
{
    public class TotalVehiclesDataPoint
    {
        public TotalVehiclesDataPoint(DateTime startTime, double vehicles)
        {
            StartTime = startTime;
            Vehicles = vehicles;
        }

        public DateTime StartTime { get; set; }
        public double Vehicles { get; set; }
    }
}
