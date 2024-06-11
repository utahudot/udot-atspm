using NetTopologySuite.Geometries;
using System;

namespace ATSPM.Application.Business.RouteSpeed
{
    public class RouteSpeed
    {
        public string RouteId { get; set; }
        public string Name { get; set; }
        public DateTime? Startdate { get; set; }
        public DateTime? Enddate { get; set; }
        public double? Avg { get; set; }
        public double? Percentilespd_15 { get; set; }
        public double? Percentilespd_85 { get; set; }
        public double? Percentilespd_95 { get; set; }
        //public int? Percentilespd_99 { get; set; }
        //(Avg speed - speed limit ) /speed limit
        public long? AverageSpeedAboveSpeedLimit { get; set; }
        // go through 15,average, 85,95, find first value above speed limit that is greater than threshold passed by user,
        // return flow * (100 - selected percentile) = estimated violation
        public long? EstimatedViolations { get; set; }
        public Geometry Shape { get; set; }  // Assuming you have a "Geometry" type to represent st_union
        public long? Flow { get; set; }
        public long SpeedLimit { get; set; }

    }
}
