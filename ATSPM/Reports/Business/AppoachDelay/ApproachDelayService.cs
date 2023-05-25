using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.AppoachDelay
{
    public class ApproachDelayService 
    {

        public ApproachDelayService()
        {
        }


        public ApproachDelayResult GetChartData(
            ApproachDelayOptions options,
            Approach approach,
            SignalPhase signalPhase)
        {            
            var dt = signalPhase.StartDate;
            var approachDelayDataPoints = new List<ApproachDelayDataPoint>();
            var approachDelayPerVehicleDataPoints = new List<ApproachDelayPerVehicleDataPoint>();
            while (dt < signalPhase.EndDate)
            {
                var endDt = dt.AddMinutes(options.BinSize);
                var pcdsInBin = signalPhase.Cycles.Where(c => c.StartTime >= dt && c.StartTime < endDt).ToList();
                var binDelay = pcdsInBin.Sum(d => d.TotalDelay);
                var binVolume = pcdsInBin.Sum(d => d.TotalVolume);
                double bindDelaypervehicle = 0;
                double bindDelayperhour = 0;

                if (binVolume > 0 && pcdsInBin.Any())
                    bindDelaypervehicle = binDelay / binVolume;
                else
                    bindDelaypervehicle = 0;

                bindDelayperhour = binDelay * (60 / options.BinSize) / 60 / 60;
                approachDelayPerVehicleDataPoints.Add(new ApproachDelayPerVehicleDataPoint(dt, bindDelaypervehicle));
                approachDelayDataPoints.Add(new ApproachDelayDataPoint(dt, bindDelayperhour));
                dt = dt.AddMinutes(options.BinSize);
            }
            var plans = GetPlans(signalPhase.Plans);
            return new ApproachDelayResult(
                approach.Id,
                approach.SignalId,
                options.GetPermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber,
                approach.Description,
                options.Start,
                options.End,
                approachDelayPerVehicleDataPoints.Average(d => d.DelayPerVehicle),
                approachDelayPerVehicleDataPoints.Sum(d => d.DelayPerVehicle),
                plans,
                approachDelayDataPoints,
                approachDelayPerVehicleDataPoints);
        }


        protected List<ApproachDelayPlan> GetPlans(List<PerdueCoordinationPlan> planCollection)
        {
            var plans = new List<ApproachDelayPlan>();
            foreach (var plan in planCollection)
            {

                var planDescription = "Unknown";
                switch (plan.PlanNumber)
                {
                    case "254":
                        planDescription = "Free";
                        break;
                    case "255":
                        planDescription = "Flash";
                        break;
                    case "0":
                        planDescription = "Unknown";
                        break;
                    default:
                        planDescription = "Plan " + plan.PlanNumber;

                        break;
                }

                var avgDelay = Math.Round(plan.AvgDelay, 0);
                var totalDelay = Math.Round(plan.TotalDelay);
                plans.Add(
                    new ApproachDelayPlan(
                        avgDelay,
                        totalDelay,
                        plan.StartTime,
                        plan.EndTime,
                        plan.PlanNumber.ToString(),
                        planDescription)
                    );
            }
            return plans;
        }
    }
}