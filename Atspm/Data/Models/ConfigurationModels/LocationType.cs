﻿#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/LocationType.cs
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

#nullable disable
using Utah.Udot.Atspm.Data.Models.ConfigurationModels;
using Utah.Udot.Atspm.Data.Relationships;

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Location type configuration
    /// </summary>
    public class LocationType : AtspmConfigModelBase<int>, IRelatedLocations
    {
        /// <summary>
        /// Name of location type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Icon for disquinguishing location type
        /// </summary>
        public string Icon { get; set; }

        #region IRelatedLocations

        /// <inheritdoc/>
        public virtual ICollection<Location> Locations { get; set; } = new HashSet<Location>();

        #endregion
    }
}
