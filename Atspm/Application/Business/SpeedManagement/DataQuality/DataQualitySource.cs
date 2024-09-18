namespace Utah.Udot.Atspm.Business.SpeedManagement.DataQuality
{
    public class DataQualitySource
    {
        public int SourceId { get; set; }
        public string Name { get; set; }
        public List<DataQualitySegment> Segments { get; set; }

    }
}