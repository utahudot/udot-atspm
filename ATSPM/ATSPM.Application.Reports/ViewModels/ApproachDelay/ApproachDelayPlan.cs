using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachDelay
{
    public class ApproachDelayPlan : Plan
    {
        public ApproachDelayPlan(
            double percentArrivalOnGreen,
            double percentGreenTime,
            double platoonRatio,
            DateTime start,
            DateTime end,
            string planNumber) : base(planNumber, start, end)
        {
            PercentArrivalOnGreen = percentArrivalOnGreen;
            PercentGreenTime = percentGreenTime;
            PlatoonRatio = platoonRatio;
        }

        public double PercentArrivalOnGreen { get; internal set; }
        public double PercentGreenTime { get; internal set; }
        public double PlatoonRatio { get; internal set; }
    }
}
