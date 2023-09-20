using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.AppoachDelay
{
    public class ApproachDelayResult : ApproachResult
    {
        public ApproachDelayResult(
            int approachId,
            string signalId,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            double averageDelayPerVehicle,
            double totalDelay,
            List<ApproachDelayPlan> plans,
            List<CycleDataPoint> approachDelayDataPoints,
            List<ApproachDelayPerVehicleDataPoint> approachDelayPerVehicleDataPoints) : base(approachId, signalId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            AverageDelayPerVehicle = averageDelayPerVehicle;
            TotalDelay = totalDelay;
            Plans = plans;
            ApproachDelayDataPoints = approachDelayDataPoints;
            ApproachDelayPerVehicleDataPoints = approachDelayPerVehicleDataPoints;
        }

        public int PhaseNumber { get; }
        public string PhaseDescription { get; }
        public double AverageDelayPerVehicle { get; }
        public double TotalDelay { get; }
        public List<ApproachDelayPlan> Plans { get; }
        public List<CycleDataPoint> ApproachDelayDataPoints { get; }
        public List<ApproachDelayPerVehicleDataPoint> ApproachDelayPerVehicleDataPoints { get; }
    }

}
