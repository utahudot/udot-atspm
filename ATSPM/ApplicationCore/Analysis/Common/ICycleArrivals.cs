using ATSPM.Data.Enums;
using System.Collections.Generic;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Defines <see cref="DataLoggerEnum.DetectorOn"/> event arrivals for cycle
    /// </summary>
    public interface ICycleArrivals : ICycle, ICycleVolume //: ICycleVolume
    {

        /// <summary>
        /// Total number of <see cref="DataLoggerEnum.DetectorOn"/> events arriving on green
        /// </summary>
        double TotalArrivalOnGreen { get; }

        /// <summary>
        /// Total number of <see cref="DataLoggerEnum.DetectorOn"/> events arriving on yellow
        /// </summary>
        double TotalArrivalOnYellow { get; }

        /// <summary>
        /// Total number of <see cref="DataLoggerEnum.DetectorOn"/> events arriving on yellow
        /// </summary>
        double TotalArrivalOnRed { get; }


        /// <summary>
        /// <see cref="DataLoggerEnum.DetectorOn"/> events arriving during cycle
        /// </summary>
        IReadOnlyList<Vehicle> Vehicles { get; }
    }
}
