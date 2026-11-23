#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityApi - Utah.Udot.ATSPM.IdentityApi.Dto/ApiKeyDtos.cs
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
    /// Data transfer object containing parameters needed to generate a new API key.
    /// </summary>
    /// <param name="Name">A descriptive name for the API key to help identify its purpose.</param>
    /// <param name="ExpiresAt">The optional expiration date and time for the API key.</param>
    /// <param name="Claims">The permissions assigned to this API key.</param>
    /// <param name="UserId">Optional target user identifier when generating a key on behalf of another user.</param>
    public record CreateApiKeyDto(
        string Name,
        DateTimeOffset? ExpiresAt,
        IEnumerable<string> Claims,
        Guid? UserId
    );

    /// <summary>
    /// Data transfer object containing the plaintext API key returned once upon creation.
    /// </summary>
    /// <param name="Name">The descriptive name of the API key.</param>
    /// <param name="PlainTextKey">The raw plaintext API key. Shown only once upon creation.</param>
    /// <param name="CreatedAt">The timestamp when the API key was created.</param>
    /// <param name="ExpiresAt">The optional expiration timestamp of the API key.</param>
    public record ApiKeyCreatedResponseDto(
        string Name,
        string PlainTextKey,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ExpiresAt
    );

    /// <summary>
    /// Data transfer object representing a basic summary of an active API key.
    /// </summary>
    /// <param name="Id">The unique identifier of the API key.</param>
    /// <param name="Name">The descriptive name of the API key.</param>
    /// <param name="CreatedAt">The timestamp when the API key was created.</param>
    /// <param name="ExpiresAt">The optional expiration timestamp of the API key.</param>
    public record ApiKeySummaryDto(
        int Id,
        string Name,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ExpiresAt
    );

    /// <summary>
    /// Data transfer object representing the complete details of an API key, including permissions.
    /// </summary>
    /// <param name="Id">The unique identifier of the API key.</param>
    /// <param name="Name">The descriptive name of the API key.</param>
    /// <param name="OwnerId">The identifier of the user who owns the API key.</param>
    /// <param name="CreatedAt">The timestamp when the API key was created.</param>
    /// <param name="ExpiresAt">The optional expiration timestamp of the API key.</param>
    /// <param name="Claims">The permissions assigned to the API key.</param>
    public record ApiKeyDetailDto(
        int Id,
        string Name,
        string OwnerId,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ExpiresAt,
        IEnumerable<string> Claims
    );
}
