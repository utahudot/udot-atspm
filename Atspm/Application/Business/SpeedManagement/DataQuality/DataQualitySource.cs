namespace Utah.Udot.Atspm.Business.SpeedManagement.DataQuality
{
    public class DataQualitySource
    {
        public int SourceId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DataQualitySegment> Segments { get; set; }

    }
}