#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Enums/DetectionTypes.cs
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
    /// Detection types
    /// </summary>
    public enum DetectionTypes
    {
        /// <summary>
        /// Not applicable
        /// </summary>
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,

        /// <summary>
        /// Basic
        /// </summary>
        [Display(Name = "Basic", Order = 1)]
        B = 1,

        /// <summary>
        /// Advanced count
        /// </summary>
        [Display(Name = "Advanced Count", Order = 2)]
        AC = 2,

        /// <summary>
        /// Advanced speed
        /// </summary>
        [Display(Name = "Advanced Speed", Order = 3)]
        AS = 3,

        /// <summary>
        /// Lane by lane count
        /// </summary>
        [Display(Name = "Lane-by-lane Count", Order = 4)]
        LLC = 4,

        /// <summary>
        /// Lane by lane with speed restriction
        /// </summary>
        [Display(Name = "Lane-by-lane with Speed Restriction", Order = 5)]
        LLS = 5,

        /// <summary>
        /// Stop bar presence
        /// </summary>
        [Display(Name = "Stop Bar Presence", Order = 6)]
        SBP = 6,

        /// <summary>
        /// Advanced presence
        /// </summary>
        [Display(Name = "Advanced Presence", Order = 7)]
        AP = 7,

        /// <summary>
        /// Passage
        /// </summary>
        [Display(Name = "Passage", Order = 8)]
        P = 8,

        /// <summary>
        /// Demand
        /// </summary>
        [Display(Name = "Demand", Order = 9)]
        D = 9,

        /// <summary>
        /// intermediate queue
        /// </summary>
        [Display(Name = "Intermediate Queue", Order = 10)]
        IQ = 10,

        /// <summary>
        /// Excessive queue
        /// </summary>
        [Display(Name = "Excessive Queue", Order = 11)]
        EQ = 11,
    }
}
