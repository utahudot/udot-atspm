using ATSPM.Data.Enums;
using System;
using System.Text.Json;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Events that coorelate to <see cref="DataLoggerEnum.DetectorOn"/>
    /// and that have been timestamp corrected for detector distances and latency
    /// using the <see cref="ATSPM.Application.AtspmMath.AdjustTimeStamp"/> calculation.
    /// </summary>
    public class CorrectedDetectorEvent : IDetectorEvent
    {
        #region IDetectorEvent

        #region ILocationPhaseLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        /// <inheritdoc/>
        public int DetectorChannel { get; set; }

        /// <inheritdoc/>
        public DirectionTypes Direction { get; set; }

        /// <summary>
        /// Coreected timestamp of event using the <see cref="ATSPM.Application.AtspmMath.AdjustTimeStamp"/> calculation.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
