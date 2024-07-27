using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.AppoachDelay
{
    public class ApproachDelayService
    {

        public ApproachDelayService()
        {
        }

        public virtual ApproachDelayResult GetChartData(
            ApproachDelayOptions options,
            PhaseDetail phaseDetail,
            LocationPhase LocationPhase)
        {
            var dt = LocationPhase.StartDate;
            var approachDelayDataPoints = new List<DataPointForDouble>();
            var approachDelayPerVehicleDataPoints = new List<DataPointForDouble>();
            while (dt < LocationPhase.EndDate)
            {
                var endDt = dt.AddMinutes(options.BinSize);
                var detectorEvents = LocationPhase.Cycles.SelectMany(c => c.DetectorEvents.Where(d => d.TimeStamp >= dt && d.TimeStamp < endDt)).ToList();
                var binDelay = detectorEvents.Where(d => d.ArrivalType == ArrivalType.ArrivalOnRed).Sum(d => d.DelaySeconds);
                double bindDelaypervehicle = 0;
                double bindDelayperhour = 0;

                if (detectorEvents.Any() && detectorEvents.Count > 0)
                    bindDelaypervehicle = binDelay / detectorEvents.Count;

                bindDelayperhour = binDelay * (60 / options.BinSize) / 60 / 60;
                approachDelayPerVehicleDataPoints.Add(new DataPointForDouble(dt, bindDelaypervehicle));
                approachDelayDataPoints.Add(new DataPointForDouble(dt, bindDelayperhour));
                dt = dt.AddMinutes(options.BinSize);
            }
            var plans = GetPlans(LocationPhase.Plans);
            return new ApproachDelayResult(
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Location.LocationIdentifier,
                phaseDetail.PhaseNumber,
                phaseDetail.Approach.Description,
                options.Start,
                options.End,
                LocationPhase.AvgDelaySeconds,
                LocationPhase.TotalDelaySeconds,
                plans,
                approachDelayDataPoints,
                approachDelayPerVehicleDataPoints);
        }


        protected static List<ApproachDelayPlan> GetPlans(List<PurdueCoordinationPlan> planCollection)
        {
            var plans = new List<ApproachDelayPlan>();
            foreach (var plan in planCollection)
            {

                string planDescription;
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
                        plan.Start,
                        plan.End,
                        plan.PlanNumber.ToString(),
                        planDescription)
                    );
            }
            return plans;
        }
    }
}