using System.Collections.Generic;

namespace ATSPM.Application.Analysis.Common
{
    public interface ICycleRatios : ICycleArrivals
    {
        double PercentArrivalOnGreen { get; }
        double PercentArrivalOnYellow { get; }
        double PercentArrivalOnRed { get; }

        double PercentGreen { get; }
        double PercentYellow { get; }
        double PercentRed { get; }

        double PlatoonRatio { get; }

        IReadOnlyList<ICycleArrivals> ArrivalCycles { get; }
    }
}
