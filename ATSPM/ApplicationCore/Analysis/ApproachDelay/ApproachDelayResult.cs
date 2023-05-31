using ATSPM.Application.Analysis.Common;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

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
            return $"Signal: {SignalId} Phase: {Phase} Start: {Start:yyyy-MM-dd'T'HH:mm:ss.f} End: {End:yyyy-MM-dd'T'HH:mm:ss.f} Avg: {AverageDelay:0.00} Total: {TotalDelay:0.0h}";
        }
    }
}
