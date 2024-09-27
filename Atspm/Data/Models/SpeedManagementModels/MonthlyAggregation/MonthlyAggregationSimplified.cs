namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;

public class MonthlyAggregationSimplified
{
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
