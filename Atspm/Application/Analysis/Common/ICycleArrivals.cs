#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Common/ICycleArrivals.cs
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

using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Analysis.Common
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
