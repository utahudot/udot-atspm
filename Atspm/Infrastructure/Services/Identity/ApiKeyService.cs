#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.Identity/ApiKeyService.cs
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

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Repositories.IdentityRepositories;
using Utah.Udot.Atspm.Services.Identity;
using Utah.Udot.Atspm.Services.Identity.Dto;
using Utah.Udot.Atspm.Infrastructure.LogMessages.Identity;

namespace Utah.Udot.Atspm.Infrastructure.Services.Identity
{
    /// <inheritdoc cref="IApiKeyService"/>
    public class ApiKeyService : IApiKeyService
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Utah.Udot.Atspm.Infrastructure.LogMessages.Identity.ApiKeyServiceLogMessages _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyService"/> class.
        /// </summary>
        /// <param name="apiKeyRepository">The API key repository.</param>
        /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
        /// <param name="logger">The logger instance.</param>
        public ApiKeyService(
            IApiKeyRepository apiKeyRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<ApiKeyService> logger)
        {
            _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _log = new Utah.Udot.Atspm.Infrastructure.LogMessages.Identity.ApiKeyServiceLogMessages(logger ?? throw new ArgumentNullException(nameof(logger)));
        }

        /// <inheritdoc/>
        public async Task<ApiKeyCreatedResponseDto> CreateKeyAsync(CreateApiKeyDto dto, ClaimsPrincipal currentUser)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "The request body could not be parsed.");
            }

            var currentUserId = _userManager.GetUserId(currentUser) ?? "Unknown";
            string userId;

            if (dto.UserId.HasValue)
            {
                var targetUserId = dto.UserId.Value.ToString();
                var targetUser = await _userManager.FindByIdAsync(targetUserId).ConfigureAwait(false);
                if (targetUser == null)
                {
                    throw new KeyNotFoundException($"User with ID '{dto.UserId}' was not found.");
                }
                userId = targetUserId;
            }
            else
            {
                userId = currentUserId;
                if (userId == "Unknown")
                {
                    throw new UnauthorizedAccessException("The authenticated user identity could not be resolved.");
                }
            }

            _log.KeyCreationInitiated(dto.Name, userId, currentUserId);

            var isGlobalAdmin = currentUser.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == AtspmAuthorization.Roles.Admin)
                                || currentUser.IsInRole(AtspmAuthorization.Roles.Admin);

            if (!isGlobalAdmin && dto.Claims != null)
            {
                foreach (var requestedClaim in dto.Claims)
                {
                    if (!currentUser.HasClaim(c => c.Type == AtspmAuthorization.RoleClaimType && c.Value == requestedClaim))
                    {
                        _log.UnauthorizedPermissionDelegated(currentUserId, requestedClaim, dto.Name);
                        throw new UnauthorizedAccessException($"You cannot grant the permission '{requestedClaim}' because you do not possess it.");
                    }
                }
            }

            var (rawKey, hash) = ApiKeyGenerator.CreateKey();

            var apiKey = new ApiKey
            {
                Name = dto.Name,
                KeyHash = hash,
                OwnerId = userId,
                ExpiresAt = dto.ExpiresAt,
                IsRevoked = false,
                Claims = dto.Claims?.Select(r => new ApiKeyClaim
                {
                    Type = ClaimTypes.Role,
                    Value = r
                }).ToList() ?? new List<ApiKeyClaim>()
            };

            await _apiKeyRepository.AddAsync(apiKey).ConfigureAwait(false);

            _log.KeyCreatedSuccessfully(apiKey.Name, apiKey.Id, apiKey.OwnerId);

            return new ApiKeyCreatedResponseDto(
                dto.Name,
                rawKey,
                apiKey.CreatedAt,
                apiKey.ExpiresAt
            );
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiKeySummaryDto>> GetKeysForUserAsync(ClaimsPrincipal currentUser)
        {
            var userId = _userManager.GetUserId(currentUser);
            if (userId == null)
            {
                throw new UnauthorizedAccessException("The authenticated user identity could not be resolved.");
            }

            _log.UserKeysRetrieved(userId);

            var keys = await _apiKeyRepository.GetActiveKeysByOwnerAsync(userId).ConfigureAwait(false);

            return keys.Select(k => new ApiKeySummaryDto(k.Id, k.Name, k.CreatedAt, k.ExpiresAt));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiKeyDetailDto>> GetAllSystemKeysAsync(ClaimsPrincipal currentUser)
        {
            var adminId = _userManager.GetUserId(currentUser) ?? "Unknown";

            _log.SystemKeysRetrieved(adminId);

            var keys = await _apiKeyRepository.GetAllActiveKeysAsync().ConfigureAwait(false);

            return keys.Select(k => new ApiKeyDetailDto(
                k.Id,
                k.Name,
                k.OwnerId,
                k.CreatedAt,
                k.ExpiresAt,
                k.Claims.Select(c => c.Value).ToList()
            ));
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeKeyAsync(int id, ClaimsPrincipal currentUser)
        {
            var userId = _userManager.GetUserId(currentUser);
            if (userId == null)
            {
                throw new UnauthorizedAccessException("The authenticated user identity could not be resolved.");
            }

            _log.KeyRevocationRequested(userId, id);

            var apiKey = await _apiKeyRepository.GetKeyWithOwnerAsync(id, userId).ConfigureAwait(false);

            if (apiKey == null)
            {
                throw new KeyNotFoundException($"API Key with ID {id} not found or access denied.");
            }

            if (apiKey.IsRevoked)
            {
                _log.KeyAlreadyRevoked(id);
                return false;
            }

            apiKey.IsRevoked = true;
            await _apiKeyRepository.UpdateAsync(apiKey).ConfigureAwait(false);

            _log.KeyRevokedSuccessfully(id);

            return true;
        }
    }
}
