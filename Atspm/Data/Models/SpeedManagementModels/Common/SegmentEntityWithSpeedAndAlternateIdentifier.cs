using ATSPM.Data.Models.SpeedManagementConfigModels;

namespace ATSPM.Data.Models.SpeedManagement.Common
{
    public class SegmentEntityWithSpeedAndAlternateIdentifier : SegmentEntity
    {
        public long SpeedLimit { get; set; }
        public string? AlternateIdentifier { get; set; }
    }
}
