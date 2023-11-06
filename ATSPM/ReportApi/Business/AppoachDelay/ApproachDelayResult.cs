using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.AppoachDelay
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
            List<DataPointForDouble> approachDelayDataPoints,
            List<DataPointForDouble> approachDelayPerVehicleDataPoints) : base(approachId, signalId, start, end)
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
        public List<DataPointForDouble> ApproachDelayDataPoints { get; }
        public List<DataPointForDouble> ApproachDelayPerVehicleDataPoints { get; }
    }

}
