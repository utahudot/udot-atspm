using ATSPM.Data.Enums;
using System.Collections.Generic;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Defines <see cref="IndianaEnumerations.VehicleDetectorOn"/> event arrivals for cycle
    /// </summary>
    public interface ICycleArrivals : ICycleTotal, ICycleVolume //: ICycleVolume
    {

        /// <summary>
        /// Total number of <see cref="IndianaEnumerations.VehicleDetectorOn"/> events arriving on green
        /// </summary>
        double TotalArrivalOnGreen { get; }

        /// <summary>
        /// Total number of <see cref="IndianaEnumerations.VehicleDetectorOn"/> events arriving on yellow
        /// </summary>
        double TotalArrivalOnYellow { get; }

        /// <summary>
        /// Total number of <see cref="IndianaEnumerations.VehicleDetectorOn"/> events arriving on yellow
        /// </summary>
        double TotalArrivalOnRed { get; }


        /// <summary>
        /// <see cref="IndianaEnumerations.VehicleDetectorOn"/> events arriving during cycle
        /// </summary>
        IReadOnlyList<Vehicle> Vehicles { get; }
    }
}
