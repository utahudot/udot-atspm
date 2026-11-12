#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/UserSetting.cs
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

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// User-configured application setting override stored in the database.
    /// </summary>
    public partial class UserSetting : AtspmConfigModelBase<int>
    {
        /// <summary>
        /// Configuration key matching the hierarchical IConfiguration path.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Overridden configuration value stored as a string.
        /// </summary>
        public string Value { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"{Key} = {Value}";
    }
}
