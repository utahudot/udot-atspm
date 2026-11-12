#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.ATSPM.ConfigApi.DTO/SystemSettingDto.cs
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

namespace Utah.Udot.ATSPM.ConfigApi.DTO
{
    /// <summary>
    /// Data transfer object representing a system-configurable application setting, its XML documentation, and its override status.
    /// </summary>
    public class SystemSettingDto
    {
        /// <summary>
        /// Hierarchical configuration key.
        /// </summary>
        [Key]
        public required string Key { get; set; }

        /// <summary>
        /// Configuration section name.
        /// </summary>
        public string? Section { get; set; }

        /// <summary>
        /// Property name on the configuration class.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Current effective value of the setting.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Human-readable description from the configuration property's XML summary comment.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Data type name of the configuration property.
        /// </summary>
        public string? ValueType { get; set; }

        /// <summary>
        /// True if the value comes from external configuration; false if overridden in the SystemSettings database table.
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
