using ATSPM.Data.Models.SpeedManagement.CongestionTracking;

namespace ATSPM.Data.Models.SpeedManagement.SpeedOverTime
{
    public class SpeedOverTimeDto : CongestionTrackingDto
    {
        public TimeOptionsEnum TimeOptions { get; set; }
    }
}
