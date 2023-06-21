using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Text.Json;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// A cycle which is the time between two <see cref="DataLoggerEnum.PhaseEndYellowChange"/> events including
    /// <see cref="DataLoggerEnum.PhaseBeginGreen"/> and <see cref="DataLoggerEnum.PhaseBeginYellowChange"/>
    /// </summary>
    public class RedToRedCycle : StartEndRange, ICycle, ISignalPhaseLayer
    {
        /// <summary>
        /// Timestamp of <see cref="DataLoggerEnum.PhaseBeginGreen"/> event
        /// </summary>
        public DateTime GreenEvent { get; set; }

        /// <summary>
        /// Timestamp of <see cref="DataLoggerEnum.PhaseBeginYellowChange"/> event
        /// </summary>
        public DateTime YellowEvent { get; set; }

        #region ISignalPhaseLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        #region ICycle

        /// <inheritdoc/>
        public double TotalGreenTime => (YellowEvent - GreenEvent).TotalSeconds;

        /// <inheritdoc/>
        public double TotalYellowTime => (End - YellowEvent).TotalSeconds;

        /// <inheritdoc/>
        public double TotalRedTime => (GreenEvent - Start).TotalSeconds;

        /// <inheritdoc/>
        public double TotalTime => (End - Start).TotalSeconds;

        #endregion

        /// <inheritdoc/>
        public override bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
