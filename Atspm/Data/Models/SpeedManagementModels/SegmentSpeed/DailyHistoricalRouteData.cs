namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed
{
    public class DailyHistoricalRouteData
    {
        public int SourceId { get; set; }
        public List<DailyAverage> DailyAverages { get; set; } = new List<DailyAverage>();
    }
}
