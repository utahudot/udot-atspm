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

    public double? WeekendAllDayAverageSpeed { get; set; } //All Day 12:00AM - 11:59PM
    public double? WeekendAllDayAverageEightyFifthSpeed { get; set; }
    public long? WeekendAllDayViolations { get; set; }
    public long? WeekendAllDayExtremeViolations { get; set; }
    public long? WeekendAllDayFlow { get; set; }
    public double? WeekendAllDayMinSpeed { get; set; }
    public double? WeekendAllDayMaxSpeed { get; set; }
    public double? WeekendAllDayVariability { get; set; }
    public double? WeekendAllDayPercentViolations { get; set; }
    public double? WeekendAllDayPercentExtremeViolations { get; set; }
    public double? WeekendAllDayAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekendAllDayEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? WeekdayAllDayAverageSpeed { get; set; } // All Day 12:00AM - 11:59PM
    public double? WeekdayAllDayAverageEightyFifthSpeed { get; set; }
    public long? WeekdayAllDayViolations { get; set; }
    public long? WeekdayAllDayExtremeViolations { get; set; }
    public long? WeekdayAllDayFlow { get; set; }
    public double? WeekdayAllDayMinSpeed { get; set; }
    public double? WeekdayAllDayMaxSpeed { get; set; }
    public double? WeekdayAllDayVariability { get; set; }
    public double? WeekdayAllDayPercentViolations { get; set; }
    public double? WeekdayAllDayPercentExtremeViolations { get; set; }
    public double? WeekdayAllDayAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekdayAllDayEightyFifthSpeedVsSpeedLimit { get; set; }


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

    public double? WeekendOffPeakAverageSpeed { get; set; } // Off Peak times
    public double? WeekendOffPeakAverageEightyFifthSpeed { get; set; }
    public long? WeekendOffPeakViolations { get; set; }
    public long? WeekendOffPeakExtremeViolations { get; set; }
    public long? WeekendOffPeakFlow { get; set; }
    public double? WeekendOffPeakMinSpeed { get; set; }
    public double? WeekendOffPeakMaxSpeed { get; set; }
    public double? WeekendOffPeakVariability { get; set; }
    public double? WeekendOffPeakPercentViolations { get; set; }
    public double? WeekendOffPeakPercentExtremeViolations { get; set; }
    public double? WeekendOffPeakAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekendOffPeakEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? WeekdayOffPeakAverageSpeed { get; set; } // Off Peak times
    public double? WeekdayOffPeakAverageEightyFifthSpeed { get; set; }
    public long? WeekdayOffPeakViolations { get; set; }
    public long? WeekdayOffPeakExtremeViolations { get; set; }
    public long? WeekdayOffPeakFlow { get; set; }
    public double? WeekdayOffPeakMinSpeed { get; set; }
    public double? WeekdayOffPeakMaxSpeed { get; set; }
    public double? WeekdayOffPeakVariability { get; set; }
    public double? WeekdayOffPeakPercentViolations { get; set; }
    public double? WeekdayOffPeakPercentExtremeViolations { get; set; }
    public double? WeekdayOffPeakAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekdayOffPeakEightyFifthSpeedVsSpeedLimit { get; set; }


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

    public double? WeekendAmPeakAverageSpeed { get; set; } // AM Peak times
    public double? WeekendAmPeakAverageEightyFifthSpeed { get; set; }
    public long? WeekendAmPeakViolations { get; set; }
    public long? WeekendAmPeakExtremeViolations { get; set; }
    public long? WeekendAmPeakFlow { get; set; }
    public double? WeekendAmPeakMinSpeed { get; set; }
    public double? WeekendAmPeakMaxSpeed { get; set; }
    public double? WeekendAmPeakVariability { get; set; }
    public double? WeekendAmPeakPercentViolations { get; set; }
    public double? WeekendAmPeakPercentExtremeViolations { get; set; }
    public double? WeekendAmPeakAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekendAmPeakEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? WeekdayAmPeakAverageSpeed { get; set; } // AM Peak times
    public double? WeekdayAmPeakAverageEightyFifthSpeed { get; set; }
    public long? WeekdayAmPeakViolations { get; set; }
    public long? WeekdayAmPeakExtremeViolations { get; set; }
    public long? WeekdayAmPeakFlow { get; set; }
    public double? WeekdayAmPeakMinSpeed { get; set; }
    public double? WeekdayAmPeakMaxSpeed { get; set; }
    public double? WeekdayAmPeakVariability { get; set; }
    public double? WeekdayAmPeakPercentViolations { get; set; }
    public double? WeekdayAmPeakPercentExtremeViolations { get; set; }
    public double? WeekdayAmPeakAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekdayAmPeakEightyFifthSpeedVsSpeedLimit { get; set; }


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

    public double? WeekendPmPeakAverageSpeed { get; set; } // PM Peak times
    public double? WeekendPmPeakAverageEightyFifthSpeed { get; set; }
    public long? WeekendPmPeakViolations { get; set; }
    public long? WeekendPmPeakExtremeViolations { get; set; }
    public long? WeekendPmPeakFlow { get; set; }
    public double? WeekendPmPeakMinSpeed { get; set; }
    public double? WeekendPmPeakMaxSpeed { get; set; }
    public double? WeekendPmPeakVariability { get; set; }
    public double? WeekendPmPeakPercentViolations { get; set; }
    public double? WeekendPmPeakPercentExtremeViolations { get; set; }
    public double? WeekendPmPeakAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekendPmPeakEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? WeekdayPmPeakAverageSpeed { get; set; } // PM Peak times
    public double? WeekdayPmPeakAverageEightyFifthSpeed { get; set; }
    public long? WeekdayPmPeakViolations { get; set; }
    public long? WeekdayPmPeakExtremeViolations { get; set; }
    public long? WeekdayPmPeakFlow { get; set; }
    public double? WeekdayPmPeakMinSpeed { get; set; }
    public double? WeekdayPmPeakMaxSpeed { get; set; }
    public double? WeekdayPmPeakVariability { get; set; }
    public double? WeekdayPmPeakPercentViolations { get; set; }
    public double? WeekdayPmPeakPercentExtremeViolations { get; set; }
    public double? WeekdayPmPeakAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekdayPmPeakEightyFifthSpeedVsSpeedLimit { get; set; }


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

    public double? WeekendMidDayAverageSpeed { get; set; } // MidDay times
    public double? WeekendMidDayAverageEightyFifthSpeed { get; set; }
    public long? WeekendMidDayViolations { get; set; }
    public long? WeekendMidDayExtremeViolations { get; set; }
    public long? WeekendMidDayFlow { get; set; }
    public double? WeekendMidDayMinSpeed { get; set; }
    public double? WeekendMidDayMaxSpeed { get; set; }
    public double? WeekendMidDayVariability { get; set; }
    public double? WeekendMidDayPercentViolations { get; set; }
    public double? WeekendMidDayPercentExtremeViolations { get; set; }
    public double? WeekendMidDayAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekendMidDayEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? WeekdayMidDayAverageSpeed { get; set; } // MidDay times
    public double? WeekdayMidDayAverageEightyFifthSpeed { get; set; }
    public long? WeekdayMidDayViolations { get; set; }
    public long? WeekdayMidDayExtremeViolations { get; set; }
    public long? WeekdayMidDayFlow { get; set; }
    public double? WeekdayMidDayMinSpeed { get; set; }
    public double? WeekdayMidDayMaxSpeed { get; set; }
    public double? WeekdayMidDayVariability { get; set; }
    public double? WeekdayMidDayPercentViolations { get; set; }
    public double? WeekdayMidDayPercentExtremeViolations { get; set; }
    public double? WeekdayMidDayAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekdayMidDayEightyFifthSpeedVsSpeedLimit { get; set; }


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

    public double? WeekendEveningAverageSpeed { get; set; } // Evening times
    public double? WeekendEveningAverageEightyFifthSpeed { get; set; }
    public long? WeekendEveningViolations { get; set; }
    public long? WeekendEveningExtremeViolations { get; set; }
    public long? WeekendEveningFlow { get; set; }
    public double? WeekendEveningMinSpeed { get; set; }
    public double? WeekendEveningMaxSpeed { get; set; }
    public double? WeekendEveningVariability { get; set; }
    public double? WeekendEveningPercentViolations { get; set; }
    public double? WeekendEveningPercentExtremeViolations { get; set; }
    public double? WeekendEveningAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekendEveningEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? WeekdayEveningAverageSpeed { get; set; } // Evening times
    public double? WeekdayEveningAverageEightyFifthSpeed { get; set; }
    public long? WeekdayEveningViolations { get; set; }
    public long? WeekdayEveningExtremeViolations { get; set; }
    public long? WeekdayEveningFlow { get; set; }
    public double? WeekdayEveningMinSpeed { get; set; }
    public double? WeekdayEveningMaxSpeed { get; set; }
    public double? WeekdayEveningVariability { get; set; }
    public double? WeekdayEveningPercentViolations { get; set; }
    public double? WeekdayEveningPercentExtremeViolations { get; set; }
    public double? WeekdayEveningAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekdayEveningEightyFifthSpeedVsSpeedLimit { get; set; }


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

    public double? WeekendEarlyMorningAverageSpeed { get; set; } // Early Morning times
    public double? WeekendEarlyMorningAverageEightyFifthSpeed { get; set; }
    public long? WeekendEarlyMorningViolations { get; set; }
    public long? WeekendEarlyMorningExtremeViolations { get; set; }
    public long? WeekendEarlyMorningFlow { get; set; }
    public double? WeekendEarlyMorningMinSpeed { get; set; }
    public double? WeekendEarlyMorningMaxSpeed { get; set; }
    public double? WeekendEarlyMorningVariability { get; set; }
    public double? WeekendEarlyMorningPercentViolations { get; set; }
    public double? WeekendEarlyMorningPercentExtremeViolations { get; set; }
    public double? WeekendEarlyMorningAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit { get; set; }

    public double? WeekdayEarlyMorningAverageSpeed { get; set; } // Early Morning times
    public double? WeekdayEarlyMorningAverageEightyFifthSpeed { get; set; }
    public long? WeekdayEarlyMorningViolations { get; set; }
    public long? WeekdayEarlyMorningExtremeViolations { get; set; }
    public long? WeekdayEarlyMorningFlow { get; set; }
    public double? WeekdayEarlyMorningMinSpeed { get; set; }
    public double? WeekdayEarlyMorningMaxSpeed { get; set; }
    public double? WeekdayEarlyMorningVariability { get; set; }
    public double? WeekdayEarlyMorningPercentViolations { get; set; }
    public double? WeekdayEarlyMorningPercentExtremeViolations { get; set; }
    public double? WeekdayEarlyMorningAvgSpeedVsSpeedLimit { get; set; }
    public double? WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit { get; set; }


    public double? PercentObserved { get; set; } //true is reliable false is unreliable
}