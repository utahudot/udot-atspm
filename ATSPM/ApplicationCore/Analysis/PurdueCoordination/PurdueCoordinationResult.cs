using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Analysis.PurdueCoordination
{
    public class PurdueCoordinationResult : StartEndRange, ICycleRatios
    {
        public PurdueCoordinationResult() {}

        public PurdueCoordinationResult(IEnumerable<PurdueCoordinationPlan> plans)
        {
            Plans = plans.ToList();
        }

        public IReadOnlyList<PurdueCoordinationPlan> Plans { get; set; } = new List<PurdueCoordinationPlan>();

        #region ICycleRatios

        public double PercentArrivalOnGreen => TotalVolume > 0 ? Math.Round(TotalArrivalOnGreen / TotalVolume * 100) : 0;

        public double PercentArrivalOnYellow => TotalVolume > 0 ? Math.Round(TotalArrivalOnYellow / TotalVolume * 100) : 0;

        public double PercentArrivalOnRed => TotalVolume > 0 ? Math.Round(TotalArrivalOnRed / TotalVolume * 100) : 0;

        public double PercentGreen => TotalVolume > 0 ? Math.Round(TotalGreenTime / TotalTime * 100) : 0;

        public double PercentYellow => TotalVolume > 0 ? Math.Round(TotalYellowTime / TotalTime * 100) : 0;

        public double PercentRed => TotalVolume > 0 ? Math.Round(TotalRedTime / TotalTime * 100) : 0;

        public double PlatoonRatio => TotalVolume > 0 ? Math.Round(PercentArrivalOnGreen / PercentGreen, 2) : 0;

        public IReadOnlyList<ICycleArrivals> ArrivalCycles => Plans.SelectMany(s => s.ArrivalCycles).ToList();

        public double TotalArrivalOnGreen => Plans.Sum(d => d.TotalArrivalOnGreen);

        public double TotalArrivalOnYellow => Plans.Sum(d => d.TotalArrivalOnYellow);

        public double TotalArrivalOnRed => Plans.Sum(d => d.TotalArrivalOnRed);

        public IReadOnlyList<Vehicle> Vehicles => Plans.SelectMany(s => s.Vehicles).ToList();

        public double TotalGreenTime => Plans.Sum(d => d.TotalGreenTime);

        public double TotalYellowTime => Plans.Sum(d => d.TotalYellowTime);

        public double TotalRedTime => Plans.Sum(d => d.TotalRedTime);

        public double TotalTime => Plans.Sum(d => d.TotalTime);

        public double TotalDelay => Plans.Sum(d => d.TotalDelay);

        public double TotalVolume => Plans.Sum(d => d.TotalVolume);

        #endregion
    }
}
