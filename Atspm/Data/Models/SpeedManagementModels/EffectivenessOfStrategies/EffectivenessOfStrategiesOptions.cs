namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.EffectivenessOfStrategies;

public class EffectivenessOfStrategiesOptions
{
    public DateTime StrategyImplementedDate { get; set; }
    public DateTime? StartTime { get; set; } = null;
    public DateTime? EndTime { get; set; } = null;
    public List<Guid> SegmentIds { get; set; }
}