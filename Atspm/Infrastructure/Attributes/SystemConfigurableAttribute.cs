#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Attributes/SystemConfigurableAttribute.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Attributes
{
    /// <summary>
    /// Marks a configuration options class as eligible to be exposed and overridden via the SystemSettings API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SystemConfigurableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemConfigurableAttribute"/> class.
        /// </summary>
        /// <param name="sectionName">Optional explicit configuration section name. Defaults to the class name if null.</param>
        public SystemConfigurableAttribute(string? sectionName = null)
        {
            SectionName = sectionName;
        }

        /// <summary>
        /// Gets the configuration section name.
        /// </summary>
        public string? SectionName { get; }
    }
}
