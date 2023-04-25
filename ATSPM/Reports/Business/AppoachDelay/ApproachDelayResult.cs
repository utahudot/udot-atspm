using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.AppoachDelay
{
    public class ApproachDelayResult
    {
        public ApproachDelayResult(
            string chartName,
            string signalId,
            string signalLocation,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            double averageDelayPerVehicle,
            double totalDelay,
            List<ApproachDelayPlan> plans,
            List<ApproachDelayDataPoint> approachDelayDataPoints,
            List<ApproachDelayPerVehicleDataPoint> approachDelayPerVehicleDataPoints)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            Start = start;
            End = end;
            AverageDelayPerVehicle = averageDelayPerVehicle;
            TotalDelay = totalDelay;
            Plans = plans;
            ApproachDelayDataPoints = approachDelayDataPoints;
            ApproachDelayPerVehicleDataPoints = approachDelayPerVehicleDataPoints;
        }

        public string ChartName { get; }
        public string SignalId { get; }
        public string SignalLocation { get; }
        public int PhaseNumber { get; }
        public string PhaseDescription { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public double AverageDelayPerVehicle { get; }
        public double TotalDelay { get; }
        public List<ApproachDelayPlan> Plans { get; }
        public List<ApproachDelayDataPoint> ApproachDelayDataPoints { get; }
        public List<ApproachDelayPerVehicleDataPoint> ApproachDelayPerVehicleDataPoints { get; }
    }

}
