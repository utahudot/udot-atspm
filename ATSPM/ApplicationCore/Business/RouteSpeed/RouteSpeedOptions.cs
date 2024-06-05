using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.RouteSpeed
{
    public class RouteSpeedOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public int ViolationThreshold { get; set; }
        public int SourceId { get; set; }
        public AnalysisPeriod AnalysisPeriod { get; set; }
    }
}
