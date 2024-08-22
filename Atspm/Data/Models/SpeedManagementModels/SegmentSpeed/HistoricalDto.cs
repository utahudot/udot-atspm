namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed
{
    public class HistoricalDTO
    {
        public Guid SegmentId { get; set; }
        public List<MonthlyHistoricalRouteData> MonthlyHistoricalRouteData { get; set; } = new List<MonthlyHistoricalRouteData>();
        public List<DailyHistoricalRouteData> DailyHistoricalRouteData { get; set; } = new List<DailyHistoricalRouteData>();
    }
}
