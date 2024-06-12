#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Common/ICycleRatios.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Data.Enums;
using System.Collections.Generic;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Defines <see cref="IndianaEnumerations.VehicleDetectorOn"/> event arrival ratios for cycle
    /// </summary>
    public interface ICycleRatios : ICycleArrivals
    {
        /// <summary>
        /// The percentage of total green arrivals vs total volume
        /// </summary>
        double PercentArrivalOnGreen { get; }

        /// <summary>
        /// The percentage of total yellow arrivals vs total volume
        /// </summary>
        double PercentArrivalOnYellow { get; }

        /// <summary>
        /// The percentage of total red arrivals vs total volume
        /// </summary>
        double PercentArrivalOnRed { get; }

        /// <summary>
        /// The percentage of green time vs total time
        /// </summary>
        double PercentGreen { get; }

        /// <summary>
        /// The percentage of yellow time vs total time
        /// </summary>
        double PercentYellow { get; }

        /// <summary>
        /// The percentage of red time vs total time
        /// </summary>
        double PercentRed { get; }

        /// <summary>
        /// The proportion of arrivals on green vs green time
        /// </summary>
        double PlatoonRatio { get; }


        /// <summary>
        /// <see cref="ICycleArrivals"/> that are used to derrive the <see cref="ICycleRatios"/>
        /// </summary>
        IReadOnlyList<ICycleArrivals> ArrivalCycles { get; }
    }
}
