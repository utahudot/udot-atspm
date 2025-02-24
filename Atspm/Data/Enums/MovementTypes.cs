#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Enums/MovementTypes.cs
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
    /// MOvement types
    /// </summary>
    public enum MovementTypes
    {
        /// <summary>
        /// Not applicable
        /// </summary>
        [Display(Name = "Unknown", Order = 6)]
        NA = 0,

        /// <summary>
        /// Thru
        /// </summary>
        [Display(Name = "Thru", Order = 3)]
        T = 1,

        /// <summary>
        /// Right
        /// </summary>
        [Display(Name = "Right", Order = 5)]
        R = 2,

        /// <summary>
        /// Left
        /// </summary>
        [Display(Name = "Left", Order = 1)]
        L = 3,

        /// <summary>
        /// Thru-right
        /// </summary>
        [Display(Name = "Thru-Right", Order = 4)]
        TR = 4,

        /// <summary>
        /// Thru left
        /// </summary>
        [Display(Name = "Thru-Left", Order = 2)]
        TL = 5,

        /// <summary>
        /// Northwest
        /// </summary>
        [Display(Name = "Northwest", Order = 6)]
        NW = 6,
        [Display(Name = "left-thru-right", Order = 7)]
        LTR = 7,
    }
}
