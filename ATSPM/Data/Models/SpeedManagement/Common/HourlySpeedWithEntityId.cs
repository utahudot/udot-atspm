using ATSPM.Data.Models.SpeedManagementAggregation;

namespace ATSPM.Data.Models.SpeedManagement.Common
{
    public class HourlySpeedWithEntityId : HourlySpeed
    {
        public long EntityId { get; set; }
        public double Length { get; set; }
    }
}
