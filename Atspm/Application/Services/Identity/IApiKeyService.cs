#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services.Identity/IApiKeyService.cs
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

using System.Security.Claims;
using Utah.Udot.Atspm.Services.Identity.Dto;

namespace Utah.Udot.Atspm.Services.Identity
{
    /// <summary>
    /// Service contract for handling system-to-system/machine integration API keys, 
    /// secure SHA-256 cryptographic validations, permissions, and revocations.
    /// </summary>
    public interface IApiKeyService
    {
        /// <summary>
        /// Generates and cryptographically stores a new API key, verifying requested claims.
        /// </summary>
        /// <param name="dto">The details of the API key to create.</param>
        /// <param name="currentUser">The ClaimsPrincipal representing the current authenticated owner.</param>
        /// <returns>A task representing the asynchronous operation, containing the plaintext key to be displayed once.</returns>
        Task<ApiKeyCreatedResponseDto> CreateKeyAsync(CreateApiKeyDto dto, ClaimsPrincipal currentUser);

        /// <summary>
        /// Retrieves all active, non-revoked API keys belonging to the authenticated user.
        /// </summary>
        /// <param name="currentUser">The ClaimsPrincipal representing the current authenticated owner.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of the owner's keys.</returns>
        Task<IEnumerable<ApiKeySummaryDto>> GetKeysForUserAsync(ClaimsPrincipal currentUser);

        /// <summary>
        /// Administrative query to retrieve all API keys in the system.
        /// </summary>
        /// <param name="currentUser">The ClaimsPrincipal representing the system administrator.</param>
        /// <returns>A task representing the asynchronous operation, containing a detailed list of all system keys.</returns>
        Task<IEnumerable<ApiKeyDetailDto>> GetAllSystemKeysAsync(ClaimsPrincipal currentUser);

        /// <summary>
        /// Marks a specific API key as revoked to prevent any future use.
        /// </summary>
        /// <param name="id">The unique identifier of the key to revoke.</param>
        /// <param name="currentUser">The ClaimsPrincipal representing the authenticated owner or admin.</param>
        /// <returns>A task representing the asynchronous operation, containing true if successful, false otherwise.</returns>
        Task<bool> RevokeKeyAsync(int id, ClaimsPrincipal currentUser);
    }
}
