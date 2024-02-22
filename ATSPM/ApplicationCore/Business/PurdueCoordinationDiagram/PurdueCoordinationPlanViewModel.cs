using ATSPM.Application.Business.Common;
using System;

namespace ATSPM.Application.Business.PurdueCoordinationDiagram
{
    public class PerdueCoordinationPlanViewModel : Plan
    {
        //public string PlanNumber { get; set; }

        public PerdueCoordinationPlanViewModel(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            double percentGreenTime,
            double percentArrivalOnGreen,
            double platoonRatio) : base(planNumber, startTime, endTime)
        {
            //PlanNumber = planNumber;
            //Start = startTime;
            PercentGreenTime = percentGreenTime;
            PercentArrivalOnGreen = percentArrivalOnGreen;
            PlatoonRatio = platoonRatio;
            //End = endTime;
        }

        //public DateTime Start { get; set; }
        //public DateTime End { get; set; }
        public double PercentGreenTime { get; set; }
        public double PercentArrivalOnGreen { get; set; }
        public double PlatoonRatio { get; set; }
    }
}