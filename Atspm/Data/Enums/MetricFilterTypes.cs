#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Enums/MetricFilterTypes.cs
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
    /// Metric filter types
    /// </summary>
    public enum MetricFilterTypes
    {
        /// <summary>
        /// Unknown
        /// </summary>
        [Display(Name = "Unknown", Order = 0)]
        Unknown,

        /// <summary>
        /// Location id
        /// </summary>
        [Display(Name = "Location Id", Order = 1)]
        locationId,

        /// <summary>
        /// Primary name
        /// </summary>
        [Display(Name = "Primary Name", Order = 2)]
        PrimaryName,

        /// <summary>
        /// Secondary name
        /// </summary>
        [Display(Name = "Secondary Name", Order = 3)]
        SecondaryName,

        /// <summary>
        /// Agency
        /// </summary>
        [Display(Name = "Agency", Order = 4)]
        Agency
    }
}
