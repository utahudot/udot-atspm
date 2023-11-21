using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// A cycle with <see cref="DataLoggerEnum.DetectorOn"/> event arrivals
    /// </summary>
    public class CycleArrivals : StartEndRange, ICycleArrivals, ISignalPhaseLayer
    {
        private readonly ICycle _cycle = new RedToRedCycle();

        public CycleArrivals() { }

        public CycleArrivals(ICycle cycle)
        {
            _cycle = cycle;
            Start = _cycle.Start;
            End = _cycle.End;

            if (cycle is ISignalPhaseLayer sp)
            {
                SignalIdentifier = sp.SignalIdentifier;
                PhaseNumber = sp.PhaseNumber;
            }
        }

        #region ISignalPhaseLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        #region ICycle

        /// <inheritdoc/>
        public double TotalGreenTime => _cycle.TotalGreenTime;

        /// <inheritdoc/>
        public double TotalYellowTime => _cycle.TotalYellowTime;

        /// <inheritdoc/>
        public double TotalRedTime => _cycle.TotalRedTime;

        /// <inheritdoc/>
        public double TotalTime => _cycle.TotalTime;

        #endregion

        #region ICycleArrivals

        /// <inheritdoc/>
        public double TotalArrivalOnGreen => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnGreen);

        /// <inheritdoc/>
        public double TotalArrivalOnYellow => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnYellow);

        /// <inheritdoc/>
        public double TotalArrivalOnRed => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnRed);

        /// <inheritdoc/>
        [JsonIgnore]
        public IReadOnlyList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        #region ICycleVolume

        /// <inheritdoc/>
        public double TotalDelay => Vehicles.Sum(d => d.Delay);

        /// <inheritdoc/>
        public double TotalVolume => Vehicles.Count(d => InRange(d.Timestamp));

        #endregion

        #endregion

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
