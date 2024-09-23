namespace Utah.Udot.Atspm.Business.SpeedManagement.DataQuality
{
    public class DataQualityOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Guid> SegmentIds { get; set; }
    }
}