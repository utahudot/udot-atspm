namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverDistance;

public class SpeedViolationsDto
{
    public Guid SegmentId { get; set; }
    public string SegmentName { get; set; }
    public long TotalFlow { get; set; }
    public long TotalViolationsCount { get; set; }
    public long TotalExtremeViolationsCount { get; set; }
    public double PercentViolations { get; set; } // Sum of SumFlow in Daily's output divided by Total Violations, converted to a Percent
    public double PercentExtremeViolations { get; set; } // Sum of SumFlow in Daily’s output divided by Total Severe Violations, converted to a Percent
    public long SpeedLimit { get; set; }
    public List<DailySpeedViolationsDto> dailySpeedViolationsDto { get; set; }
}

public class DailySpeedViolationsDto
{
    public DateTime Date { get; set; }
    public long DailyFlow { get; set; }
    public long DailyViolationsCount { get; set; }
    public long DailyExtremeViolationsCount { get; set; }
    public double DailyPercentViolations { get; set; } //SumViolationsCount / SumFlow
    public double DailyPercentExtremeViolations { get; set; } //SumSevereViolationsCount / SumFlow
}
