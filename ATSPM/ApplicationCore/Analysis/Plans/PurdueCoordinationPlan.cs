using ATSPM.Application.Analysis.Common;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Analysis.Plans
{
    public class PurdueCoordinationPlan : Plan, ICycleRatios
    {
        private readonly List<ICycleArrivals> _arrivalCycles = new();

        public PurdueCoordinationPlan() { }

        public IReadOnlyList<ICycleArrivals> ArrivalCycles => _arrivalCycles;

        #region ICycleArrivals

        public double TotalArrivalOnGreen => _arrivalCycles.Sum(d => d.TotalArrivalOnGreen);

        public double TotalArrivalOnYellow => _arrivalCycles.Sum(d => d.TotalArrivalOnYellow);

        public double TotalArrivalOnRed => _arrivalCycles.Sum(d => d.TotalArrivalOnRed);

        public double TotalGreenTime => _arrivalCycles.Sum(d => d.TotalGreenTime);

        public double TotalYellowTime => _arrivalCycles.Sum(d => d.TotalYellowTime);

        public double TotalRedTime => _arrivalCycles.Sum(d => d.TotalRedTime);

        public double TotalTime => _arrivalCycles.Sum(d => d.TotalTime);

        public IReadOnlyList<Vehicle> Vehicles => _arrivalCycles.SelectMany(s => s.Vehicles).ToList();

        #endregion

        #region ICycleRatios

        public double PercentArrivalOnGreen => TotalVolume > 0 ? Math.Round(TotalArrivalOnGreen / TotalVolume * 100) : 0;

        public double PercentArrivalOnYellow => TotalVolume > 0 ? Math.Round(TotalArrivalOnYellow / TotalVolume * 100) : 0;

        public double PercentArrivalOnRed => TotalVolume > 0 ? Math.Round(TotalArrivalOnRed / TotalVolume * 100) : 0;

        public double PercentGreen => TotalVolume > 0 ? Math.Round(TotalGreenTime / TotalTime * 100) : 0;

        public double PercentYellow => TotalVolume > 0 ? Math.Round(TotalYellowTime / TotalTime * 100) : 0;

        public double PercentRed => TotalVolume > 0 ? Math.Round(TotalRedTime / TotalTime * 100) : 0;

        public double PlatoonRatio => TotalVolume > 0 ? Math.Round(PercentArrivalOnGreen / PercentGreen, 2) : 0;

        #endregion

        #region ICycleVolume

        public double TotalDelay => _arrivalCycles.Sum(d => d.TotalDelay);
        public double TotalVolume => _arrivalCycles.Sum(d => d.TotalVolume);

        #endregion

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

        public override bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }
    }
}
