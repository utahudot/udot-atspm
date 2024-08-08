namespace ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;

public class MonthlyAggregation
{
    public Guid? Id { get; set; }
    public DateTime? CreatedDate { get; set; } //This is the timestamp of when this entry was created.
    public DateTime BinStartTime { get; set; } // This is the 
    public Guid SegmentId { get; set; }
    public long SourceId { get; set; }
    public double? AllDayAverageSpeed { get; set; } //All Day 12:00AM - 11:59PM
    public long? AllDayViolations { get; set; }
    public long? AllDayExtremeViolations { get; set; }
    public double? OffPeakAverageSpeed { get; set; } //Off Peak 10:00PM - 4:00AM
    public long? OffPeakViolations { get; set; }
    public long? OffPeakExtremeViolations { get; set; }
    public double? AmPeakAverageSpeed { get; set; } //AM Peak 6:00AM - 9:00AM
    public long? AmPeakViolations { get; set; }
    public long? AmPeakExtremeViolations { get; set; }
    public double? PmPeakAverageSpeed { get; set; } //PM Peak 4:00PM - 6:00PM
    public long? PmPeakViolations { get; set; }
    public long? PmPeakExtremeViolations { get; set; }
    public double? MidDayAverageSpeed { get; set; } //Mid Day 9:00AM - 4:00PM
    public long? MidDayViolations { get; set; }
    public long? MidDayExtremeViolations { get; set; }
    public double? EveningAverageSpeed { get; set; } //Evening 6:00PM - 10:00PM
    public long? EveningViolations { get; set; }
    public long? EveningExtremeViolations { get; set; }
    public double? EarlyMorningAverageSpeed { get; set; } //4:00AM - 6:00AM
    public long? EarlyMorningViolations { get; set; }
    public long? EarlyMorningExtremeViolations { get; set; }
    public bool DataQuality { get; set; } //true is reliable false is unreliable
}