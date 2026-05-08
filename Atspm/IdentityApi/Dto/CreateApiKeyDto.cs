#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityApi - Utah.Udot.ATSPM.IdentityApi.Dto/CreateApiKeyDto.cs
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

namespace Utah.Udot.ATSPM.IdentityApi.Dto
{
    /// <summary>
    /// Data transfer object used for providing the necessary information to create a new API key.
    /// </summary>
    public class CreateApiKeyDto
    {
        /// <summary>
        /// Gets or sets a descriptive name for the API key to help identify its purpose.
        /// </summary>
        /// <example>Internal Integration Service</example>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the optional expiration date and time for the API key. 
        /// If null, the key may be treated as having no expiration, depending on system policy.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the list of permissions assigned to this API key.
        /// </summary>
        /// <value>A list of strings representing claim names.</value>
        public List<string> Claims { get; set; }
    }
}
