namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.EffectivenessOfStrategies;

public class EffectivenessOfStrategiesDto
{
    public Guid SegmentId { get; set; }
    public string SegmentName { get; set; }
    public double ChangeInAverageSpeed { get; set; } //Overall Change in Average Speed = After’s Weighted Average Speed – Before’s Weighted Average Speed
    public double ChangeInEightyFifthPercentileSpeed { get; set; } //Overall Change in 85th Percentile Speed = After’s Weighted 85th Percentile Speed – Before’s Weighted 85th Percentile Speed
    public double ChangeInVariablitiy { get; set; } //Overall Change in Variability = After’s Total Speed Variability – Before’s Total Speed Variability
    public double ChangeInPercentViolations { get; set; } //Overall Change in Percent Violations = After’s Percent Violations – Before’s Percent Violations
    public double ChangeInPercentExtremeViolations { get; set; } //Overall Change in Percent Severe Violations = After’s Percent Severe Violations – Before’s Percent Severe Violations
    public long SpeedLimit { get; set; }
    public List<TimeSegmentEffectiveness> WeeklyEffectiveness { get; set; }
    public TimeSegmentEffectiveness Before { get; set; }
    public TimeSegmentEffectiveness After { get; set; }

}

public class TimeSegmentEffectiveness
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long Flow { get; set; }
    public double MinSpeed { get; set; }
    public double MaxSpeed { get; set; }
    public double AverageSpeed { get; set; } //Average of the AvgSpeed column, weighted by the Flow column
    public double AverageEightyFifthSpeed { get; set; } //Average of the 85thPercentileSpeed column, weighted by the Flow column
    public double Variablitiy { get; set; } //Maximum of the MaxSpeed column minus minimum of the MinSpeed column
    public double PercentViolations { get; set; } //Sum of the ViolationsCount column divided by the sum of the Flow column
    public double PercentExtremeViolations { get; set; } //Sum of the ExtremeViolationsCount column divided by the sum of the Flow column
}

