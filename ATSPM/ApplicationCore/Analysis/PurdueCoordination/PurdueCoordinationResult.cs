using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ATSPM.Application.Analysis.PurdueCoordination
{
    /// <summary>
    /// Purdue coordination results
    /// </summary>
    public class PurdueCoordinationResult : StartEndRange, ICycleRatios, ISignalPhaseLayer
    {
        public PurdueCoordinationResult() {}

        public PurdueCoordinationResult(IEnumerable<PurdueCoordinationPlan> plans)
        {
            Plans = plans.ToList();
        }

        public IReadOnlyList<PurdueCoordinationPlan> Plans { get; set; } = new List<PurdueCoordinationPlan>();

        #region ISignalPhaseLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        #region ICycle

        /// <inheritdoc/>
        public double TotalGreenTime => Plans.Sum(d => d.TotalGreenTime);

        /// <inheritdoc/>
        public double TotalYellowTime => Plans.Sum(d => d.TotalYellowTime);

        /// <inheritdoc/>
        public double TotalRedTime => Plans.Sum(d => d.TotalRedTime);

        /// <inheritdoc/>
        public double TotalTime => Plans.Sum(d => d.TotalTime);

        #endregion

        #region ICycleArrivals


        /// <inheritdoc/>
        public double TotalArrivalOnGreen => Plans.Sum(d => d.TotalArrivalOnGreen);

        /// <inheritdoc/>
        public double TotalArrivalOnYellow => Plans.Sum(d => d.TotalArrivalOnYellow);

        /// <inheritdoc/>
        public double TotalArrivalOnRed => Plans.Sum(d => d.TotalArrivalOnRed);

        /// <inheritdoc/>
        public IReadOnlyList<Vehicle> Vehicles => Plans.SelectMany(s => s.Vehicles).ToList();

        #endregion

        #region ICycleRatios

        /// <inheritdoc/>
        public double PercentArrivalOnGreen => TotalVolume > 0 ? Math.Round(TotalArrivalOnGreen / TotalVolume * 100) : 0;

        /// <inheritdoc/>
        public double PercentArrivalOnYellow => TotalVolume > 0 ? Math.Round(TotalArrivalOnYellow / TotalVolume * 100) : 0;

        /// <inheritdoc/>
        public double PercentArrivalOnRed => TotalVolume > 0 ? Math.Round(TotalArrivalOnRed / TotalVolume * 100) : 0;

        /// <inheritdoc/>
        public double PercentGreen => TotalVolume > 0 ? Math.Round(TotalGreenTime / TotalTime * 100) : 0;

        /// <inheritdoc/>
        public double PercentYellow => TotalVolume > 0 ? Math.Round(TotalYellowTime / TotalTime * 100) : 0;

        /// <inheritdoc/>
        public double PercentRed => TotalVolume > 0 ? Math.Round(TotalRedTime / TotalTime * 100) : 0;

        /// <inheritdoc/>
        public double PlatoonRatio => TotalVolume > 0 ? Math.Round(PercentArrivalOnGreen / PercentGreen, 2) : 0;

        /// <inheritdoc/>
        public IReadOnlyList<ICycleArrivals> ArrivalCycles => Plans.SelectMany(s => s.ArrivalCycles).ToList();

        #endregion

        #region ICycleVolume

        /// <inheritdoc/>
        public double TotalDelay => Plans.Sum(d => d.TotalDelay);

        /// <inheritdoc/>
        public double TotalVolume => Plans.Sum(d => d.TotalVolume);

        #endregion

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
