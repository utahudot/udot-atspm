using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramAverageOptions
    {
        public int RouteId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public List<LocationWithSequence> Sequence { get; set; }
        public List<LocationWithCoordPhases> CoordinatedPhases { get; set; }
        public int[] DaysOfWeek { get; set; }
        public int? SpeedLimit { get; set; }
    }
}
