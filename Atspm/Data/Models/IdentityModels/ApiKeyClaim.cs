#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models.IdentityModels/ApiKeyClaim.cs
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

namespace Utah.Udot.Atspm.Data.Models.IdentityModels
{
    /// <summary>
    /// Represents an individual claim or permission assigned to an <see cref="ApiKey"/>.
    /// </summary>
    public class ApiKeyClaim
    {
        /// <summary>
        /// Gets or sets the unique identifier for the API key claim.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key identifier for the associated <see cref="ApiKey"/>.
        /// </summary>
        public int ApiKeyId { get; set; }

        /// <summary>
        /// Gets or sets the type of the claim (e.g., "role", "permission", or "scope").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value associated with the claim type (e.g., "Admin", "Read-Only").
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
}
