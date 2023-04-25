using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.YellowRedActivations
{
    public class RedLightMonitorPlanCollectionData
    {
        public double Violations
        {
            get { return PlanList.Sum(d => d.Violations); }
        }
        public List<RLMPlan> PlanList { get; } = new List<RLMPlan>();
        public double SRLVSeconds { get; }
        public Approach Approach { get; set; }
    }
}