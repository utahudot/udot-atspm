using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.IdentityRepositories
{
    /// <summary>
    /// Repository interface for accessing and querying <see cref="ApiKey"/> entities.
    /// </summary>
    public interface IApiKeyRepository : IAsyncRepository<ApiKey>
    {
        /// <summary>
        /// Retrieves active (non-revoked) API keys for a specific owner.
        /// </summary>
        Task<IEnumerable<ApiKey>> GetActiveKeysByOwnerAsync(string ownerId);
        /// <summary>
        /// Retrieves all active (non-revoked) API keys in the system.
        /// </summary>
        Task<IEnumerable<ApiKey>> GetAllActiveKeysAsync();
        /// <summary>
        /// Retrieves an API key by ID and owner, verifying ownership.
        /// </summary>
        Task<ApiKey?> GetKeyWithOwnerAsync(int id, string ownerId);
        /// <summary>
        /// Retrieves an active, non-expired API key along with its claims by key hash.
        /// Useful for the Authentication Handler.
        /// </summary>
        Task<ApiKey?> GetActiveKeyByHashAsync(string keyHash);
    }
}
