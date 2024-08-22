using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedFromImpact;

public class SpeedFromImpactDto
{
    public List<Impact>? Impacts { get; set; }
    public List<Segment>? Segments { get; set; }
    public List<HourlySpeed>? HourlySpeeds { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}