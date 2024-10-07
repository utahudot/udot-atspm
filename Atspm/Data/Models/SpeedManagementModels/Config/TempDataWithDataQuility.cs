namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config
{
    public class TempDataWithDataQuility
    {
        public DateTime BinStartTime { get; set; }
        public double Average { get; set; }
        public double DataQuality { get; set; }
        public long EntityId { get; set; }
    }
}
