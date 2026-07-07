#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityApi - Utah.Udot.ATSPM.IdentityApi.Dto/ApiKeyMetadataDto.cs
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
    /// Metadata returned for API keys. The raw key is intentionally excluded.
    /// </summary>
    public class ApiKeyMetadataDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string OwnerId { get; set; } = string.Empty;

        public string OwnerEmail { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        public List<string> Claims { get; set; } = new();
    }
}
