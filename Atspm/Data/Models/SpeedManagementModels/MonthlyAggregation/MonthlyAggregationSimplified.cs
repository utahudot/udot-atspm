using System.Reflection;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;

public class MonthlyAggregationSimplified
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; } //This is the timestamp of when this entry was created.
    public DateTime BinStartTime { get; set; } // This is the 
    public Guid SegmentId { get; set; }
    public long SourceId { get; set; }

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

public enum TimePeriodFilter
{
    [DisplayName("AllDay")]
    AllDay,

    [DisplayName("OffPeak")]
    OffPeak,

    [DisplayName("AMPeak")]
    AmPeak,

    [DisplayName("PMPeak")]
    PmPeak,

    [DisplayName("MidDay")]
    MidDay,

    [DisplayName("Evening")]
    Evening,

    [DisplayName("EarlyMorning")]
    EarlyMorning
}

public enum MonthAggClassification
{
    [DisplayName("Total")]
    Total,

    [DisplayName("Weekend")]
    Weekend,

    [DisplayName("Weekday")]
    Weekday
}

public enum SpeedCategoryFilter
{
    [DisplayName("Average Speed")]
    AverageSpeed,

    [DisplayName("Average 85th Percentile Speed")]
    AverageEightyFifthSpeed,

    [DisplayName("Violations")]
    Violations,

    [DisplayName("Extreme Violations")]
    ExtremeViolations,

    [DisplayName("Flow")]
    Flow,

    [DisplayName("Minimum Speed")]
    MinSpeed,

    [DisplayName("Maximum Speed")]
    MaxSpeed,

    [DisplayName("Variability")]
    Variability,

    [DisplayName("Percentage of Violations")]
    PercentViolations,

    [DisplayName("Percentage of Extreme Violations")]
    PercentExtremeViolations,

    [DisplayName("Average Speed vs Speed Limit")]
    AvgSpeedVsSpeedLimit,

    [DisplayName("85th Percentile Speed vs Speed Limit")]
    EightyFifthSpeedVsSpeedLimit,

    [DisplayName("Percentage Observed")]
    PercentObserved
}

public class MonthlyAggregationOptions
{
    public SpeedCategoryFilter category { get; set; }
    public TimePeriodFilter timePeriod { get; set; }
    public MonthAggClassification aggClassification { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long? SourceId { get; set; } = null;
    public string? Region { get; set; } = null;
    public string? City { get; set; } = null;
    public string? County { get; set; } = null;
    public string? Order { get; set; } = "DESC";
    public int? Limit { get; set; } = 25;
}


/// <summary>
/// ENUM HELPERS
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class DisplayNameAttribute : Attribute
{
    public string Name { get; }

    public DisplayNameAttribute(string name)
    {
        Name = name;
    }
}
public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field.GetCustomAttribute<DisplayNameAttribute>();

        return attribute != null ? attribute.Name : value.ToString();
    }
}
public class EnumMapping
{
    public int Number { get; set; }
    public string DisplayName { get; set; }
}
