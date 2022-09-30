using System;

namespace ATSPM.Application.Reports.ViewModels.YellowRedActivations
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

        public int TotalViolations { get; set; }
        public int SevereViolations { get; set; }
        public double PercentViolations { get; set; }
        public double PercentSevereViolations { get; set; }
        public double AverageTimeViolations { get; set; }
    }
}