using System.Collections.Generic;

namespace ATSPM.Application.Analysis.Common
{
    public interface ICycleArrivals : ICycle, ICycleVolume //: ICycleVolume
    {
        double TotalArrivalOnGreen { get; }
        double TotalArrivalOnYellow { get; }
        double TotalArrivalOnRed { get; }

        IReadOnlyList<Vehicle> Vehicles { get; }
    }
}
