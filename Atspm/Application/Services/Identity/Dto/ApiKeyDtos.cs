#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services.Identity.Dto/ApiKeyDtos.cs
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

namespace Utah.Udot.Atspm.Services.Identity.Dto
{
    /// <summary>
    /// Data transfer object containing parameters needed to generate a new API key.
    /// </summary>
    public record CreateApiKeyDto(
        string Name,
        DateTimeOffset? ExpiresAt,
        IEnumerable<string> Claims,
        Guid? UserId
    );

    /// <summary>
    /// Data transfer object containing the plaintext API key returned once upon creation.
    /// </summary>
    public record ApiKeyCreatedResponseDto(
        string Name,
        string PlainTextKey,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ExpiresAt
    );

    /// <summary>
    /// Data transfer object representing a basic summary of an active API key.
    /// </summary>
    public record ApiKeySummaryDto(
        int Id,
        string Name,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ExpiresAt
    );

    /// <summary>
    /// Data transfer object representing the complete details of an API key, including permissions.
    /// </summary>
    public record ApiKeyDetailDto(
        int Id,
        string Name,
        string OwnerId,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ExpiresAt,
        IEnumerable<string> Claims
    );
}
