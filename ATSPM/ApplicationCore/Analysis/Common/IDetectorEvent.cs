using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Events that coorelate to <see cref="DataLoggerEnum.DetectorOn"/>
    /// and that have been timestamp corrected for detector distances and latency
    /// using the <see cref="ATSPM.Application.AtspmMath.AdjustTimeStamp"/> calculation.
    /// </summary>
    public interface IDetectorEvent : ILocationPhaseLayer, ILocationDetectorLayer, ITimestamp
    {
        /// <summary>
        /// Direction of travel when the event occured.
        /// </summary>
        DirectionTypes Direction { get; set; }
    }
}
