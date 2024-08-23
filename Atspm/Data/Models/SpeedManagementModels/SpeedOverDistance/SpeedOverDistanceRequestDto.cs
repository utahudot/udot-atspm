namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverDistance;

public class SpeedOverDistanceRequestDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Guid> SegmentIds { get; set; }
}