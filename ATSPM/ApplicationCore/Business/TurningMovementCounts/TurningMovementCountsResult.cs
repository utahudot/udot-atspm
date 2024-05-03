using ATSPM.Application.Business.TurningMovementCounts.MOE.Common.Business.TMC;
using System.Collections.Generic;

namespace ATSPM.Application.Business.TurningMovementCounts
{
    /// <summary>
    /// Turning Movement Count chart
    /// </summary>
    public class TurningMovementCountsResult
    {
        public List<TurningMovementCountsLanesResult> Charts { get; set; }
        public List<TurningMovementCountData> Table { get; set; }
    }
}