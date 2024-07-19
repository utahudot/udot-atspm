using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.RouteSpeed
{
    public class HistoricalSpeedOptions
    {
        public int RouteId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public int Violation {  get; set; }
    }
}
