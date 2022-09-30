using System;

namespace ATSPM.Application.Reports.ViewModels.SplitFail
{
    public class SplitFailPlan : Plan
    {
        public SplitFailPlan(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            DateTime fails,
            string percentFails) : base(planNumber, startTime, endTime)
        {
            Fails = fails;
            PercentFails = percentFails;
        }

        public DateTime Fails { get; internal set; }
        public string PercentFails { get; internal set; }
    }
}