using System;

namespace ATSPM.Application.Reports.Business.Common
{
    public class PerdueCoordinationPlanViewModel
    {
        public string PlanNumber { get; set; }

        public PerdueCoordinationPlanViewModel(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            double percentGreenTime,
            double percentArrivalOnGreen,
            double platoonRatio)
        {
            PlanNumber = planNumber;
            StartTime = startTime;
            PercentGreenTime = percentGreenTime;
            PercentArrivalOnGreen = percentArrivalOnGreen;
            PlatoonRatio = platoonRatio;
            EndTime = endTime;
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double PercentGreenTime { get; set; }
        public double PercentArrivalOnGreen { get; set; }
        public double PlatoonRatio { get; set; }
    }
}