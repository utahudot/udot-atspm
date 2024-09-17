namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.DataQuality
{
    public class DataQualityOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Guid> SegmentIds { get; set; }
    }
}