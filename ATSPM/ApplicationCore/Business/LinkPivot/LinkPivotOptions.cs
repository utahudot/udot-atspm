using ATSPM.Application.Business.Common;
using System;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotOptions
    {
        public int RouteId { get; set; }
        public int CycleLength { get; set; }
        public string Direction { get; set; }
        public double Bias { get; set; }
        public string BiasDirection { get; set; }
        public int[] DaysOfWeek { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}