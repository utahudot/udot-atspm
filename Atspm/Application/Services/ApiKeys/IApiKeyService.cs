using System.Security.Claims;

namespace Utah.Udot.Atspm.Services.ApiKeys
{
    /// <summary>
    /// Service interface for coordinating API key business operations.
    /// </summary>
    public interface IApiKeyService
    {
        /// <summary>
        /// Generates and stores a new API key, verifying ownership/permissions.
        /// </summary>
        Task<ApiKeyCreatedResponseDto> CreateKeyAsync(CreateApiKeyDto dto, ClaimsPrincipal currentUser);

        /// <summary>
        /// Retrieves all active, non-revoked API keys belonging to the authenticated user.
        /// </summary>
        Task<IEnumerable<ApiKeySummaryDto>> GetKeysForUserAsync(ClaimsPrincipal currentUser);

        /// <summary>
        /// Retrieves all active, non-revoked API keys in the system.
        /// </summary>
        Task<IEnumerable<ApiKeyDetailDto>> GetAllSystemKeysAsync(ClaimsPrincipal currentUser);

        /// <summary>
        /// Marks a specific API key as revoked to prevent further use.
        /// </summary>
        Task<bool> RevokeKeyAsync(int id, ClaimsPrincipal currentUser);
    }
}
