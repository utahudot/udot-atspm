namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;

public class MonthlyAggregationSimplified
{
    public Guid? Id { get; set; }
    public DateTime? CreatedDate { get; set; } //This is the timestamp of when this entry was created.
    public DateTime? BinStartTime { get; set; } // This is the 
    public Guid? SegmentId { get; set; }
    public long? SourceId { get; set; }

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

public enum TimePeriod
{
    AllDay,
    OffPeak,
    AmPeak,
    PmPeak,
    MidDay,
    Evening,
    EarlyMorning
}

public enum DayType
{
    Total,
    Weekend,
    Weekday
}

public class MonthlyAggregationReducedTime
{
    public MonthlyAggregationSimplified Total { get; set; }
    public MonthlyAggregationSimplified Weekday { get; set; }
    public MonthlyAggregationSimplified Weekend { get; set; }
}
