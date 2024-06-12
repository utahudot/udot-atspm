#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.AppoachDelay/ApproachDelayService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
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
                //var pcdsInBin = LocationPhase.Cycles.Where(c => c.StartTime >= dt && c.StartTime < endDt).ToList();
                var binDelay = detectorEvents.Where(d => d.ArrivalType == ArrivalType.ArrivalOnRed).Sum(d => d.DelaySeconds);
                //var binVolume = pcdsInBin.Sum(d => d.TotalVolume);
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
                        plan.End,
                        plan.PlanNumber.ToString(),
                        planDescription)
                    );
            }
            return plans;
        }
    }
}