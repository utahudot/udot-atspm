using System.Collections.Generic;

namespace ATSPM.Application.Business.RouteSpeed
{
    public class DailyHistoricalRouteData
    {
        public int SourceId { get; set; }
        public List<DailyAverage> DailyAverages { get; set; } = new List<DailyAverage>();
    }
}
