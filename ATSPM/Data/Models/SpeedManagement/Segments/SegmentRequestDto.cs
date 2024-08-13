namespace ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;

public class SegmentRequestDto
{
    public List<Guid>? SegmentIds { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}