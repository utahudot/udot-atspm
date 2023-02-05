using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.ViewModels.ApproachDelay;
using ATSPM.Application.Repositories;
using Legacy.Common.Business;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.AppoachDelay
{
    public class ApproachDelayService
    {
        private readonly ISignalRepository signalRepository;
        private readonly PlanService planService;
        private readonly SignalPhaseService signalPhaseService;
        private readonly IApproachRepository approachRepository;

        public ApproachDelayService(
            ISignalRepository signalRepository,
            PlanService planService,
            SignalPhaseService signalPhaseService,
            IApproachRepository approachRepository)
        {
            this.signalRepository = signalRepository;
            this.planService = planService;
            this.signalPhaseService = signalPhaseService;
            this.approachRepository = approachRepository;
        }


        public ApproachDelayResult GetChartData(
            ApproachDelayOptions options)
        {
            var approach = approachRepository.Lookup(options.ApproachId);
            var signalPhase = signalPhaseService.GetSignalPhaseData(
                options.StartDate,
                options.EndDate,
                options.GetPermissivePhase,
                false,
                null,
                options.BinSize,
                options.MetricTypeId,
                approach
                );
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalId, options.StartDate);
            var dt = signalPhase.StartDate;
            List<ApproachDelayDataPoint> approachDelayDataPoints = new List<ApproachDelayDataPoint>();
            List<ApproachDelayPerVehicleDataPoint> approachDelayPerVehicleDataPoints = new List<ApproachDelayPerVehicleDataPoint>();
            while (dt < signalPhase.EndDate)
            {
                var pcdsInBin = from item in signalPhase.Cycles
                                where item.StartTime >= dt && item.StartTime < dt.AddMinutes(options.BinSize)
                                select item;

                var binDelay = pcdsInBin.Sum(d => d.TotalDelay);
                var binVolume = pcdsInBin.Sum(d => d.TotalVolume);
                double bindDelaypervehicle = 0;
                double bindDelayperhour = 0;

                if (binVolume > 0 && pcdsInBin.Any())
                    bindDelaypervehicle = binDelay / binVolume;
                else
                    bindDelaypervehicle = 0;

                bindDelayperhour = binDelay * (60 / options.BinSize) / 60 / 60;

                if (options.ShowDelayPerVehicle)
                    approachDelayPerVehicleDataPoints.Add(new ApproachDelayPerVehicleDataPoint(dt, bindDelaypervehicle));
                if (options.ShowDelayPerHour)
                    approachDelayDataPoints.Add(new ApproachDelayDataPoint(dt, bindDelayperhour));

                dt = dt.AddMinutes(options.BinSize);
            }
            var plans = GetPlans(signalPhase.Plans, options.StartDate, options.ShowPlanStatistics);
            return new ApproachDelayResult(
                "Approach Delay",
                approach.SignalId,
                approach.Signal.SignalDescription(),
                options.GetPermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber,
                approach.Description,
                options.StartDate,
                options.EndDate,
                approachDelayPerVehicleDataPoints.Average(d => d.DelayPerVehicle),
                approachDelayPerVehicleDataPoints.Sum(d => d.DelayPerVehicle),
                plans,
                approachDelayDataPoints,
                approachDelayPerVehicleDataPoints
                );
        }


        protected List<ApproachDelayPlan> GetPlans(List<PerdueCoordinationPlan> planCollection, DateTime graphStartDate,
            bool showPlanStatistics)
        {
            var plans = new List<ApproachDelayPlan>();
            foreach (var plan in planCollection)
            {

                var planDescription = "Unknown";
                switch (plan.PlanNumber)
                {
                    case 254:
                        planDescription = "Free";
                        break;
                    case 255:
                        planDescription = "Flash";
                        break;
                    case 0:
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