using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Repositories.IdentityRepositories;
using Utah.Udot.Atspm.Services.ApiKeys;

namespace Utah.Udot.Atspm.Infrastructure.Services
{
    /// <inheritdoc cref="IApiKeyService"/>
    public class ApiKeyService : IApiKeyService
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApiKeyServiceLogMessages _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyService"/> class.
        /// </summary>
        /// <param name="apiKeyRepository">The API key repository.</param>
        /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
        /// <param name="logger">The logger instance.</param>
        public ApiKeyService(IApiKeyRepository apiKeyRepository, UserManager<ApplicationUser> userManager, ILogger<ApiKeyService> logger)
        {
            _apiKeyRepository = apiKeyRepository;
            _userManager = userManager;
            _log = new ApiKeyServiceLogMessages(logger);
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

            // 1. Resolve target user ID
            if (dto.UserId.HasValue)
            {
                var targetUserId = dto.UserId.Value.ToString();
                var targetUser = await _userManager.FindByIdAsync(targetUserId);
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

            await _apiKeyRepository.AddAsync(apiKey);

            _log.KeyCreatedSuccessfully(apiKey.Name, apiKey.Id, apiKey.OwnerId);

            return new ApiKeyCreatedResponseDto(rawKey, "Copy this key now. For security reasons, it cannot be retrieved again.");
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

            var keys = await _apiKeyRepository.GetActiveKeysByOwnerAsync(userId);

            return keys.Select(k => new ApiKeySummaryDto(k.Id, k.Name, k.CreatedAt, k.ExpiresAt));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiKeyDetailDto>> GetAllSystemKeysAsync(ClaimsPrincipal currentUser)
        {
            var adminId = _userManager.GetUserId(currentUser) ?? "Unknown";

            _log.SystemKeysRetrieved(adminId);

            var keys = await _apiKeyRepository.GetAllActiveKeysAsync();

            return keys.Select(k => new ApiKeyDetailDto(k.Id, k.Name, k.OwnerId, k.CreatedAt, k.ExpiresAt));
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

            var apiKey = await _apiKeyRepository.GetKeyWithOwnerAsync(id, userId);

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
            await _apiKeyRepository.UpdateAsync(apiKey);

            _log.KeyRevokedSuccessfully(id);

            return true;
        }
    }
}
