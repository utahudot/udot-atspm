using SpeedManagementDataDownloader.Common.HourlySpeeds;

namespace SpeedManagementDataDownloader.Common.Dtos
{
    public class HourlySpeedWithEntityId : HourlySpeed
    {
        public long EntityId { get; set; }
    }
}
