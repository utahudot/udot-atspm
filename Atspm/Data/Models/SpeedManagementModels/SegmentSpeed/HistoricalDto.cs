using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.RouteSpeed
{
    public class HistoricalDTO
    {
        public Guid SegmentId { get; set; }
        public List<MonthlyHistoricalRouteData> MonthlyHistoricalRouteData { get; set; } = new List<MonthlyHistoricalRouteData>();
        public List<DailyHistoricalRouteData> DailyHistoricalRouteData { get; set; } = new List<DailyHistoricalRouteData>();
    }
}
