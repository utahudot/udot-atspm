using ATSPM.Data.Models.SpeedManagement.CongestionTracking;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverTime
{
    public class SpeedOverTimeDto : CongestionTrackingDto
    {
        public TimeOptionsEnum TimeOptions { get; set; }
    }
}
