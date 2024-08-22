using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common
{
    public class SegmentEntityWithSpeed : SegmentEntity
    {
        public long SpeedLimit { get; set; }
    }
}
