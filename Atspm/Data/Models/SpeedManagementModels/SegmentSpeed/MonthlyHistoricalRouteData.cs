
namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed
{
    public class MonthlyHistoricalRouteData
    {
        public int SourceId { get; set; }
        public List<MonthlyAverage> MonthlyAverages { get; set; } = new List<MonthlyAverage>();
    }
}
