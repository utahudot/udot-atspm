#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/UserSettingDescriptor.cs
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

using System.Reflection;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Contains reflection and XML documentation metadata for a property on a <see cref="Utah.Udot.Atspm.Infrastructure.Attributes.UserConfigurableAttribute"/> options class.
    /// </summary>
    public class UserSettingDescriptor
    {
        /// <summary>
        /// Hierarchical configuration key.
        /// </summary>
        public required string Key { get; init; }

        /// <summary>
        /// Configuration section name.
        /// </summary>
        public required string Section { get; init; }

        /// <summary>
        /// Property name on the configuration class.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// CLR type name of the property.
        /// </summary>
        public required string ValueType { get; init; }

        /// <summary>
        /// Human-readable description extracted from the property's XML documentation comment.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Default value defined on a new instance of the configuration class.
        /// </summary>
        public string? CodeDefaultValue { get; init; }

        /// <summary>
        /// Cached reflection property info used for type conversion and binding.
        /// </summary>
        public required PropertyInfo PropertyInfo { get; init; }
    }
}
