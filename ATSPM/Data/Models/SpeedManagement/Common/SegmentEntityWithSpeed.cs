using ATSPM.Data.Models.SpeedManagementConfigModels;

namespace ATSPM.Data.Models.SpeedManagement.Common
{
    public class SegmentEntityWithSpeed : SegmentEntity
    {
        public long SpeedLimit { get; set; }
    }
}
