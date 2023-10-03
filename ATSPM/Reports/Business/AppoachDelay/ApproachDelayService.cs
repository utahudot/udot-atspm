using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
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
            PhaseDetail phaseDetail,
            SignalPhase signalPhase)
        {
            var dt = signalPhase.StartDate;
            var approachDelayDataPoints = new List<DataPointSeconds>();
            var approachDelayPerVehicleDataPoints = new List<ApproachDelayPerVehicleDataPoint>();
            while (dt < signalPhase.EndDate)
            {
                var endDt = dt.AddMinutes(options.BinSize);
                var detectorEvents = signalPhase.Cycles.SelectMany(c => c.DetectorEvents.Where(d => d.TimeStamp >= dt && d.TimeStamp < endDt)).ToList();
                //var pcdsInBin = signalPhase.Cycles.Where(c => c.StartTime >= dt && c.StartTime < endDt).ToList();
                var binDelay = detectorEvents.Where(d => d.ArrivalType == ArrivalType.ArrivalOnRed).Sum(d => d.DelaySeconds);
                //var binVolume = pcdsInBin.Sum(d => d.TotalVolume);
                double bindDelaypervehicle = 0;
                double bindDelayperhour = 0;

                if (detectorEvents.Any() && detectorEvents.Count > 0)
                    bindDelaypervehicle = binDelay / detectorEvents.Count;

                bindDelayperhour = binDelay * (60 / options.BinSize) / 60 / 60;
                approachDelayPerVehicleDataPoints.Add(new ApproachDelayPerVehicleDataPoint(dt, bindDelaypervehicle));
                approachDelayDataPoints.Add(new DataPointSeconds(dt, bindDelayperhour));
                dt = dt.AddMinutes(options.BinSize);
            }
            var plans = GetPlans(signalPhase.Plans);
            return new ApproachDelayResult(
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Signal.SignalIdentifier,
                phaseDetail.PhaseNumber,
                phaseDetail.Approach.Description,
                options.Start,
                options.End,
                signalPhase.AvgDelaySeconds,
                signalPhase.TotalDelaySeconds,
                plans,
                approachDelayDataPoints,
                approachDelayPerVehicleDataPoints);
        }


        protected List<ApproachDelayPlan> GetPlans(List<PurdueCoordinationPlan> planCollection)
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
                        plan.Start,
                        plan.EndTime,
                        plan.PlanNumber.ToString(),
                        planDescription)
                    );
            }
            return plans;
        }
    }
}