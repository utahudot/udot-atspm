using ATSPM.Data.Models;
using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.YellowRedActivations
{
    public class YellowRedActivationsPhaseData
    {
        public bool GetPermissivePhase { get; set; }

        public VolumeCollection Volume { get; }

        public int PhaseNumber { get; set; }

        public double Violations
        {
            get { return Plans.Sum(d => d.Violations); }
        }

        public IList<YellowRedActivationPlan> Plans { get; set; }

        public List<YellowRedActivationsCycle> Cycles { get; set; }

        public double SevereRedLightViolationSeconds { get; set; }

        public double SevereRedLightViolations
        {
            get { return Plans.Sum(d => d.SevereRedLightViolations); }
        }

        public double TotalVolume { get; set; }

        public double PercentViolations
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(Violations / TotalVolume * 100, 0);
                return 0;
            }
        }

        public double PercentSevereViolations
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(SevereRedLightViolations / TotalVolume * 100, 2);
                return 0;
            }
        }

        public double YellowOccurrences
        {
            get { return Plans.Sum(d => d.YellowOccurrences); }
        }

        public double TotalYellowTime
        {
            get { return Plans.Sum(d => d.TotalYellowTime); }
        }

        public double AverageTYLO => Math.Round(TotalYellowTime / YellowOccurrences, 1);

        public double PercentYellowOccurrences
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(YellowOccurrences / TotalVolume * 100, 0);
                return 0;
            }
        }

        public double ViolationTime
        {
            get { return Plans.Sum(p => p.ViolationTime); }
        }

        public Approach Approach { get; set; }
    }
}