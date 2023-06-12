using ATSPM.Application.Analysis.Common;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Analysis.Plans
{
    /// <summary>
    /// Plan specific to purdue coordination information
    /// </summary>
    public class PurdueCoordinationPlan : Plan, ICycleRatios
    {
        private readonly List<ICycleArrivals> _arrivalCycles = new();

        public PurdueCoordinationPlan() { }

        #region ICycle

        /// <inheritdoc/>
        public double TotalGreenTime => _arrivalCycles.Sum(d => d.TotalGreenTime);

        /// <inheritdoc/>
        public double TotalYellowTime => _arrivalCycles.Sum(d => d.TotalYellowTime);

        /// <inheritdoc/>
        public double TotalRedTime => _arrivalCycles.Sum(d => d.TotalRedTime);

        /// <inheritdoc/>
        public double TotalTime => _arrivalCycles.Sum(d => d.TotalTime);

        #endregion

        #region ICycleArrivals

        /// <inheritdoc/>
        public double TotalArrivalOnGreen => _arrivalCycles.Sum(d => d.TotalArrivalOnGreen);

        /// <inheritdoc/>
        public double TotalArrivalOnYellow => _arrivalCycles.Sum(d => d.TotalArrivalOnYellow);

        /// <inheritdoc/>
        public double TotalArrivalOnRed => _arrivalCycles.Sum(d => d.TotalArrivalOnRed);

        /// <inheritdoc/>
        public IReadOnlyList<Vehicle> Vehicles => _arrivalCycles.SelectMany(s => s.Vehicles).ToList();

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
        public IReadOnlyList<ICycleArrivals> ArrivalCycles => _arrivalCycles;

        #endregion

        #region ICycleVolume

        /// <inheritdoc/>
        public double TotalDelay => _arrivalCycles.Sum(d => d.TotalDelay);

        /// <inheritdoc/>
        public double TotalVolume => _arrivalCycles.Sum(d => d.TotalVolume);

        #endregion

        #region IPlan

        /// <inheritdoc/>
        public override bool TryAssignToPlan(IStartEndRange range)
        {
            if (InRange(range.Start) && InRange(range.End))
            {
                if (range is ICycleArrivals cycle)
                    _arrivalCycles.Add(cycle);

                return true;
            }

            return false;
        }

        #endregion

        #region IStartEndRange

        /// <inheritdoc/>
        public override bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }

        #endregion
    }
}
