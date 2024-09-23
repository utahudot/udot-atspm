namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedCompliance;

public class SpeedComplianceOptions
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Guid> SegmentIds { get; set; }
}