using ATSPM.Application.Reports.ViewModels;
using System;

namespace ATSPM.Application.Reports.Business.ArrivalOnRed
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