﻿namespace ATSPM.Data.Models.SpeedConfigModels;

public class MonthlySpeed
{
    public DateOnly Date { get; set; }
    public TimeOnly BinStartTime { get; set; }
    public string SegmentId { get; set; }
    public int SourceId { get; set; }
    public int ConfidenceId { get; set; }
    public int Average { get; set; }
    public int? FifteenthSpeed { get; set; }
    public int? EightyFifthSpeed { get; set; }
    public int? NinetyFifthSpeed { get; set; }
    public int? NinetyNinthSpeed { get; set; }
    public int? Violation { get; set; }
    public int? Flow { get; set; }

}