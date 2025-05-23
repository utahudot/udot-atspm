﻿#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/MeasureComment.cs
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

// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.Atspm.Data.Models.ConfigurationModels;
using Utah.Udot.Atspm.Data.Relationships;

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Measure comment for tracking Location measures
    /// </summary>
    public partial class MeasureComment : AtspmConfigModelBase<int>, IRelatedMeasureTypes, ILocationLayer
    {
        /// <summary>
        /// Creation timestamp of comment
        /// </summary>
        public DateTime TimeStamp { get; set; }
        
        /// <summary>
        /// Comment value
        /// </summary>
        public string Comment { get; set; }

        #region ILocationLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        #endregion

        #region IRelatedMeasureTypes

        /// <inheritdoc/>
        public virtual ICollection<MeasureType> MeasureTypes { get; set; } = new HashSet<MeasureType>();

        #endregion

        /// <inheritdoc/>
        public override string ToString() => $"{Id} - {LocationIdentifier} - {TimeStamp} - {Comment}";
    }
}