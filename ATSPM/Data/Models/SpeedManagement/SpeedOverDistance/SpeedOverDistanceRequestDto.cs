namespace ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;

public class SpeedOverDistanceRequestDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Guid> SegmentIds { get; set; }
}