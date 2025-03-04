#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Enums/LaneTypes.cs
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

using System.ComponentModel.DataAnnotations;

namespace Utah.Udot.Atspm.Data.Enums
{
    /// <summary>
    /// Lane types
    /// </summary>
    public enum LaneTypes
    {
        /// <summary>
        /// Not applicable
        /// </summary>
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,

        /// <summary>
        /// Vehicle
        /// </summary>
        [Display(Name = "Vehicle")]
        V = 1,

        /// <summary>
        /// Bike
        /// </summary>
        [Display(Name = "Bike")]
        Bike = 2,

        /// <summary>
        /// Pedestrian
        /// </summary>
        [Display(Name = "Pedestrian")]
        Ped = 3,

        /// <summary>
        /// Exit
        /// </summary>
        [Display(Name = "Exit")]
        E = 4,

        /// <summary>
        /// Light rail transit
        /// </summary>
        [Display(Name = "Light Rail Transit")]
        LRT = 5,

        /// <summary>
        /// Bus
        /// </summary>
        [Display(Name = "Bus")]
        Bus = 6,

        /// <summary>
        /// High occupancy vechile
        /// </summary>
        [Display(Name = "High Occupancy Vehicle")]
        HDV = 7,
    }
}
