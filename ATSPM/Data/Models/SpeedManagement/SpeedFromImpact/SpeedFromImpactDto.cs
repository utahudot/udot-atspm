using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Data.Models.SpeedManagementConfigModels;

namespace ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;

public class SpeedFromImpactDto
{
    public List<Impact>? Impacts { get; set; }
    public List<Segment>? Segments { get; set; }
    public List<HourlySpeed>? HourlySpeeds { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}