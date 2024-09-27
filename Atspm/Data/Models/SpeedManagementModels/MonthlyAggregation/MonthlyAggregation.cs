namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;

public class MonthlyAggregation
{
    public Guid? Id { get; set; }
    public DateTime? CreatedDate { get; set; } //This is the timestamp of when this entry was created.
    public DateTime BinStartTime { get; set; } // This is the 
    public Guid SegmentId { get; set; }
    public long SourceId { get; set; }

    public double? AllDayAverageSpeed { get; set; } //All Day 12:00AM - 11:59PM
    public double? AllDayAverageEightyFifthSpeed { get; set; }
    public long? AllDayViolations { get; set; }
    public long? AllDayExtremeViolations { get; set; }
    public long? AllDayFlow { get; set; }
    public double? AllDayMinSpeed { get; set; }
    public double? AllDayMaxSpeed { get; set; }
    public double? AllDayVariability { get; set; }
    public double? AllDayPercentViolations { get; set; }
    public double? AllDayPercentExtremeViolations { get; set; }
    public double? AllDayAvgSpeedVsSpeedLimit { get; set; }
    public double? AllDayEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? OffPeakAverageSpeed { get; set; } //Off Peak 10:00PM - 4:00AM
    public double? OffPeakAverageEightyFifthSpeed { get; set; }
    public long? OffPeakViolations { get; set; }
    public long? OffPeakExtremeViolations { get; set; }
    public long? OffPeakFlow { get; set; }
    public double? OffPeakMinSpeed { get; set; }
    public double? OffPeakMaxSpeed { get; set; }
    public double? OffPeakVariability { get; set; }
    public double? OffPeakPercentViolations { get; set; }
    public double? OffPeakPercentExtremeViolations { get; set; }
    public double? OffPeakAvgSpeedVsSpeedLimit { get; set; }
    public double? OffPeakEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? AmPeakAverageSpeed { get; set; } //AM Peak 6:00AM - 9:00AM
    public double? AmPeakAverageEightyFifthSpeed { get; set; }
    public long? AmPeakViolations { get; set; }
    public long? AmPeakExtremeViolations { get; set; }
    public long? AmPeakFlow { get; set; }
    public double? AmPeakMinSpeed { get; set; }
    public double? AmPeakMaxSpeed { get; set; }
    public double? AmPeakVariability { get; set; }
    public double? AmPeakPercentViolations { get; set; }
    public double? AmPeakPercentExtremeViolations { get; set; }
    public double? AmPeakAvgSpeedVsSpeedLimit { get; set; }
    public double? AmPeakEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? PmPeakAverageSpeed { get; set; } //PM Peak 4:00PM - 6:00PM
    public double? PmPeakAverageEightyFifthSpeed { get; set; }
    public long? PmPeakViolations { get; set; }
    public long? PmPeakExtremeViolations { get; set; }
    public long? PmPeakFlow { get; set; }
    public double? PmPeakMinSpeed { get; set; }
    public double? PmPeakMaxSpeed { get; set; }
    public double? PmPeakVariability { get; set; }
    public double? PmPeakPercentViolations { get; set; }
    public double? PmPeakPercentExtremeViolations { get; set; }
    public double? PmPeakAvgSpeedVsSpeedLimit { get; set; }
    public double? PmPeakEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? MidDayAverageSpeed { get; set; } //Mid Day 9:00AM - 4:00PM
    public double? MidDayAverageEightyFifthSpeed { get; set; }
    public long? MidDayViolations { get; set; }
    public long? MidDayExtremeViolations { get; set; }
    public long? MidDayFlow { get; set; }
    public double? MidDayMinSpeed { get; set; }
    public double? MidDayMaxSpeed { get; set; }
    public double? MidDayVariability { get; set; }
    public double? MidDayPercentViolations { get; set; }
    public double? MidDayPercentExtremeViolations { get; set; }
    public double? MidDayAvgSpeedVsSpeedLimit { get; set; }
    public double? MidDayEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? EveningAverageSpeed { get; set; } //Evening 6:00PM - 10:00PM
    public double? EveningAverageEightyFifthSpeed { get; set; }
    public long? EveningViolations { get; set; }
    public long? EveningExtremeViolations { get; set; }
    public long? EveningFlow { get; set; }
    public double? EveningMinSpeed { get; set; }
    public double? EveningMaxSpeed { get; set; }
    public double? EveningVariability { get; set; }
    public double? EveningPercentViolations { get; set; }
    public double? EveningPercentExtremeViolations { get; set; }
    public double? EveningAvgSpeedVsSpeedLimit { get; set; }
    public double? EveningEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? EarlyMorningAverageSpeed { get; set; } //4:00AM - 6:00AM
    public double? EarlyMorningAverageEightyFifthSpeed { get; set; }
    public long? EarlyMorningViolations { get; set; }
    public long? EarlyMorningExtremeViolations { get; set; }
    public long? EarlyMorningFlow { get; set; }
    public double? EarlyMorningMinSpeed { get; set; }
    public double? EarlyMorningMaxSpeed { get; set; }
    public double? EarlyMorningVariability { get; set; }
    public double? EarlyMorningPercentViolations { get; set; }
    public double? EarlyMorningPercentExtremeViolations { get; set; }
    public double? EarlyMorningAvgSpeedVsSpeedLimit { get; set; }
    public double? EarlyMorningEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? PercentObserved { get; set; } //true is reliable false is unreliable
}