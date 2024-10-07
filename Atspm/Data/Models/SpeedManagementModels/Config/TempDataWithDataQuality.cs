namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config
{
    public class TempDataWithDataQuality
    {
        public DateTime BinStartTime { get; set; }
        public double Average { get; set; }
        public double DataQuality { get; set; }
        public string EntityId { get; set; }
    }
}
