using System;

namespace ATSPM.ReportApi.Business.AppoachDelay
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
