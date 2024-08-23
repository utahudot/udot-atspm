using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common
{
    public class SegmentEntityWithSpeedAndAlternateIdentifier : SegmentEntity
    {
        public long SpeedLimit { get; set; }
        public string? AlternateIdentifier { get; set; }
    }
}
