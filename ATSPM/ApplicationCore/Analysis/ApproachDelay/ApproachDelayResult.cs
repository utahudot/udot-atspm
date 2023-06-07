using ATSPM.Application.Analysis.Common;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ATSPM.Application.Analysis.ApproachDelay
{
    public class ApproachDelayResult : StartEndRange
    {
        //public string ChartName { get; }
        public string SignalId { get; set; }
        //public string SignalLocation { get; }
        public int Phase { get; set; }
        //public string PhaseDescription { get; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public double AverageDelay { get; set; }
        public double TotalDelay { get; set; }

        //public double AverageDelay => Vehicles.Average(a => a.Delay);
        //public double TotalDelay => Vehicles.Sum(s => s.Delay) / 3600;
        //public List<ApproachDelayPlan> Plans { get; }
        //public List<ApproachDelayDataPoint> ApproachDelayDataPoints { get; }
        //public List<ApproachDelayPerVehicleDataPoint> ApproachDelayPerVehicleDataPoints { get; }

        //public List<Vehicle> Vehicles { get; set; } = new();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
