namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Segments;

public class SegmentRequestDto
{
    public List<Guid>? SegmentIds { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<long>? SourceIds { get; set; }
}