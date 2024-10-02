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

public enum FilteringTimePeriod
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


public class MonthlyAggregationOptions
{
    public FilteringTimePeriod timePeriod { get; set; }
    public MonthAggClassification aggClassification { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

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
    public string LetterName { get; set; }
}
