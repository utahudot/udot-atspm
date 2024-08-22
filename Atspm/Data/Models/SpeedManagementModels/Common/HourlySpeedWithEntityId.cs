namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common
{
    public class HourlySpeedWithEntityId : HourlySpeed
    {
        public long EntityId { get; set; }
        public double Length { get; set; }
    }
}
