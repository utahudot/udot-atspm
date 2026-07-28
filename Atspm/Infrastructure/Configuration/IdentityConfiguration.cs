#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/IdentityConfiguration.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration values used by the Identity API for identity workflows and generated links.
    /// </summary>
    [ConfigurationSection(nameof(IdentityConfiguration), "Configuration for identity API behavior")]
    public class IdentityConfiguration
    {
        /// <summary>
        /// Public website URL used when building account and identity links.
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// Default email address used by identity workflows.
        /// </summary>
        public string DefaultEmailAddress { get; set; }
    }
}
