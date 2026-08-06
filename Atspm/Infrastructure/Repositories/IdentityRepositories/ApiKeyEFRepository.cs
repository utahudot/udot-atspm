using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Repositories.IdentityRepositories;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.IdentityRepositories
{
    ///<inheritdoc cref="IApiKeyRepository"/>
    public class ApiKeyEFRepository : ATSPMRepositoryEFBase<ApiKey>, IApiKeyRepository
    {
        /// <inheritdoc/>
        public ApiKeyEFRepository(IdentityContext db, ILogger<ApiKeyEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region IApiKeyRepository

        /// <inheritdoc/>
        public async Task<ApiKey> GetActiveKeyByHashAsync(string keyHash)
        {
            return await table
                .Include(k => k.Claims)
                .FirstOrDefaultAsync(k => k.KeyHash == keyHash
                && !k.IsRevoked
                && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiKey?>> GetActiveKeysByOwnerAsync(string ownerId)
        {
            return await table
                .AsNoTracking()
                .Where(k => k.OwnerId == ownerId && !k.IsRevoked)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApiKey>> GetAllActiveKeysAsync()
        {
            return await table
                .AsNoTracking()
                .Where(k => !k.IsRevoked)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<ApiKey?> GetKeyWithOwnerAsync(int id, string ownerId)
        {
            return await table.FirstOrDefaultAsync(k => k.Id == id && k.OwnerId == ownerId);
        }

        #endregion
    }
}
