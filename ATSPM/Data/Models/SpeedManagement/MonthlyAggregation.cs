namespace ATSPM.Data.Models.SpeedManagementAggregation;

public class MonthlyAggregation
{
    public Guid? Id { get; set; }
    public DateTime CreatedDate { get; set; } //This is the timestamp of when this entry was created.
    public DateTime BinStartTime { get; set; } // This is the 
    public Guid SegmentId { get; set; }
    public int SourceId { get; set; }
    public int? AllDayAverageSpeed { get; set; } //All Day 12:00AM - 11:59PM
    public int? AllDayViolations { get; set; }
    public int? AllDayExtremeViolations { get; set; }
    public int? OffPeakAverageSpeed { get; set; } //Off Peak 10:00PM - 4:00AM
    public int? OffPeakViolations { get; set; }
    public int? OffPeakExtremeViolations { get; set; }
    public int? AmPeakAverageSpeed { get; set; } //AM Peak 6:00AM - 9:00AM
    public int? AmPeakViolations { get; set; }
    public int? AmPeakExtremeViolations { get; set; }
    public int? PmPeakAverageSpeed { get; set; } //PM Peak 4:00PM - 6:00PM
    public int? PmPeakViolations { get; set; }
    public int? PmPeakExtremeViolations { get; set; }
    public int? MidDayAverageSpeed { get; set; } //Mid Day 9:00AM - 4:00PM
    public int? MidDayViolations { get; set; }
    public int? MidDayExtremeViolations { get; set; }
    public int? EveningAverageSpeed { get; set; } //Evening 6:00PM - 10:00PM
    public int? EveningViolations { get; set; }
    public int? EveningExtremeViolations { get; set; }
    public int? EarlyMorningAverageSpeed { get; set; } //4:00AM - 6:00AM
    public int? EarlyMorningViolations { get; set; }
    public int? EarlyMorningExtremeViolations { get; set; }
    public bool DataQuality { get; set; } //true is reliable false is unreliable
}