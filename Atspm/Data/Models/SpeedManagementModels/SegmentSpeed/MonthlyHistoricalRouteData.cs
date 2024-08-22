using System.Collections.Generic;

namespace ATSPM.Application.Business.RouteSpeed
{
    public class MonthlyHistoricalRouteData
    {
        public int SourceId { get; set; }
        public List<MonthlyAverage> MonthlyAverages { get; set; } = new List<MonthlyAverage>();
    }
}
