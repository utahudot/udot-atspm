using ATSPM.Application.Business.Common;
using System;

namespace ATSPM.Application.Business.YellowRedActivations
{
    public class YellowRedActivationsPlan : Plan
    {
        public YellowRedActivationsPlan(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            int totalViolations,
            int severeViolations,
            double percentViolations,
            double percentSevereViolations,
            double averageTimeViolations) : base(planNumber, startTime, endTime)
        {
            TotalViolations = totalViolations;
            SevereViolations = severeViolations;
            PercentViolations = percentViolations;
            PercentSevereViolations = percentSevereViolations;
            AverageTimeViolations = averageTimeViolations;
        }

        public int TotalViolations { get; internal set; }
        public int SevereViolations { get; internal set; }
        public double PercentViolations { get; internal set; }
        public double PercentSevereViolations { get; internal set; }
        public double AverageTimeViolations { get; internal set; }
    }
}