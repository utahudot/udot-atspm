namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels;

public class HourlySpeed
{
    public DateTime Date { get; set; }
    public DateTime BinStartTime { get; set; }
    public Guid SegmentId { get; set; }
    public long SourceId { get; set; }
    public double? PercentObserved { get; set; }
    public double Average { get; set; }
    public double? FifteenthSpeed { get; set; }
    public double? EightyFifthSpeed { get; set; }
    public double? NinetyFifthSpeed { get; set; }
    public double? NinetyNinthSpeed { get; set; }
    public long? Violation { get; set; }
    public long? ExtremeViolation { get; set; }
    public long? Flow { get; set; }
    public double? MinSpeed { get; set; }
    public double? MaxSpeed { get; set; }
}