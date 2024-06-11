using System.Collections.Generic;

namespace ATSPM.Application.Business.RouteSpeed
{
    public class HistoricalDTO
    {
        public int RouteId { get; set; }
        public List<MonthlyHistoricalRouteData> MonthlyHistoricalRouteData { get; set; } = new List<MonthlyHistoricalRouteData>();
        public List<DailyHistoricalRouteData> DailyHistoricalRouteData { get; set; } = new List<DailyHistoricalRouteData>();
    }
}
