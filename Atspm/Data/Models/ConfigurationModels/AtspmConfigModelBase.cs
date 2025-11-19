#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models.ConfigurationModels/AtspmConfigModelBase.cs
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

using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.NetStandardToolkit.BaseClasses;

namespace Utah.Udot.Atspm.Data.Models.ConfigurationModels
{
    /// <summary>
    /// Base class for configuration context models.
    /// This base includes interfaces for working with user interfaces.
    /// </summary>
    public class AtspmConfigModelBase<T> : ObservableObjectBase, IAuditProperties, ICloneable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public T Id { get; set; } = default!;

        #region IAuditProperties

        /// <inheritdoc/>
        public DateTime? Created { get; set; }

        /// <inheritdoc/>
        public DateTime? Modified { get; set; }

        /// <inheritdoc/>
        public string? CreatedBy { get; set; }

        /// <inheritdoc/>
        public string? ModifiedBy { get; set; }

        #endregion

        #region IClonable

        ///<inheritdoc/>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
