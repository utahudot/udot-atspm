using ATSPM.Application.Business.TurningMovementCounts.MOE.Common.Business.TMC;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.TurningMovementCounts
{
    /// <summary>
    /// Turning Movement Count chart
    /// </summary>
    public class TurningMovementCountsResult
    {
        public KeyValuePair<DateTime, int> PeakHour { get; set; }
        public List<TurningMovementCountsLanesResult> Charts { get; set; }
        public List<TurningMovementCountData> Table { get; set; }
        public double PeakHourFactor { get; set; }
    }
}