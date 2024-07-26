namespace ATSPM.Data.Models.SpeedManagementAggregation;

public class HourlySpeed
{
    public DateTime Date { get; set; }
    public DateTime BinStartTime { get; set; }
    public string SegmentId { get; set; }
    public long SourceId { get; set; }
    public long ConfidenceId { get; set; }
    public long Average { get; set; }
    public long? FifteenthSpeed { get; set; }
    public long? EightyFifthSpeed { get; set; }
    public long? NinetyFifthSpeed { get; set; }
    public long? NinetyNinthSpeed { get; set; }
    public long? Violation { get; set; }
    public long? ExtremeViolation { get; set; }
    public long? Flow { get; set; }

}