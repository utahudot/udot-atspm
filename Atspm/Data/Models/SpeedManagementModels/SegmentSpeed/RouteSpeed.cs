using NetTopologySuite.Geometries;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed
{
    //public class RouteSpeedHourlyAggregation
    //{
    //    public string SegmentId { get; set; }
    //    public long SourceId { get; set; }
    //    public string Name { get; set; }
    //    public DateTime? Startdate { get; set; }
    //    public DateTime? Enddate { get; set; }
    //    public double? Avg { get; set; }
    //    public double? Percentilespd_15 { get; set; }
    //    public double? Percentilespd_85 { get; set; }
    //    public double? Percentilespd_95 { get; set; }
    //    public double? MinSpeed { get; set; }
    //    public double? MaxSpeed { get; set; }
    //    //public int? Percentilespd_99 { get; set; }
    //    //(Avg speed - speed limit ) /speed limit
    //    public long? AverageSpeedAboveSpeedLimit { get; set; }
    //    // go through 15,average, 85,95, find first value above speed limit that is greater than threshold passed by user,
    //    // return flow * (100 - selected percentile) = estimated violation
    //    public Double? EstimatedViolations { get; set; }
    //    public Geometry Shape { get; set; }  // Assuming you have a "Geometry" type to represent st_union
    //    public long? Flow { get; set; }
    //    public long SpeedLimit { get; set; }
    //    public Double? EstimatedExtremeViolations { get; set; }
    //}

    public class RouteSpeed
    {
        public DateTime CreatedDate { get; set; }
        public DateTime BinStartTime { get; set; }
        public Guid SegmentId { get; set; }
        public long SourceId { get; set; }
        public string Name { get; set; }
        public Geometry Shape { get; set; }
        public long SpeedLimit { get; set; }
        public string? Region { get; set; } = null;
        public string? City { get; set; } = null;
        public string? County { get; set; } = null;

        public double? AverageSpeed { get; set; }
        public double? AverageEightyFifthSpeed { get; set; }
        public long? Violations { get; set; }
        public long? ExtremeViolations { get; set; }
        public long? Flow { get; set; }
        public double? MinSpeed { get; set; }
        public double? MaxSpeed { get; set; }
        public double? Variability { get; set; }
        public double? PercentViolations { get; set; }
        public double? PercentExtremeViolations { get; set; }
        public double? AvgSpeedVsSpeedLimit { get; set; }
        public double? EightyFifthSpeedVsSpeedLimit { get; set; }
        public double? PercentObserved { get; set; }

    }
}
