using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ATSPM.ReportApi.Business.AppoachDelay
{
    public class ApproachDelayResult : ApproachResult
    {
        public ApproachDelayResult()
        {
            
        }

        public ApproachDelayResult(
            int approachId,
            string locationId,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            double averageDelayPerVehicle,
            double totalDelay,
            List<ApproachDelayPlan> plans,
            List<DataPointForDouble> approachDelayDataPoints,
            List<DataPointForDouble> approachDelayPerVehicleDataPoints) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            AverageDelayPerVehicle = averageDelayPerVehicle;
            TotalDelay = totalDelay;
            Plans = plans;
            ApproachDelayDataPoints = approachDelayDataPoints;
            ApproachDelayPerVehicleDataPoints = approachDelayPerVehicleDataPoints;
        }

        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public double AverageDelayPerVehicle { get; set; }
        public double TotalDelay { get; set; }

        [JsonIgnore]
        public List<ApproachDelayPlan> Plans { get; set; }

        [JsonIgnore]
        public List<DataPointForDouble> ApproachDelayDataPoints { get; set; }

        [JsonIgnore]
        public List<DataPointForDouble> ApproachDelayPerVehicleDataPoints { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

}
