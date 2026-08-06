#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories/UserGeographyEFRepository.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories
{
    /// <inheritdoc cref="IUserGeographyRepository"/>
    public class UserGeographyEFRepository : IUserGeographyRepository
    {
        private readonly ConfigContext _db;
        private readonly ILogger<UserGeographyEFRepository> _log;

        /// <summary>
        /// Constructor for UserGeographyEFRepository
        /// </summary>
        public UserGeographyEFRepository(ConfigContext db, ILogger<UserGeographyEFRepository> log)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserArea>> GetAreasByUserIdsAsync(IEnumerable<string> userIds)
        {
            if (userIds == null) return Enumerable.Empty<UserArea>();

            var idList = userIds.ToList();
            return await _db.UserAreas
                .AsNoTracking()
                .Where(x => idList.Contains(x.UserId))
                .Include(x => x.Area)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserRegion>> GetRegionsByUserIdsAsync(IEnumerable<string> userIds)
        {
            if (userIds == null) return Enumerable.Empty<UserRegion>();

            var idList = userIds.ToList();
            return await _db.UserRegions
                .AsNoTracking()
                .Where(x => idList.Contains(x.UserId))
                .Include(x => x.Region)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserJurisdiction>> GetJurisdictionsByUserIdsAsync(IEnumerable<string> userIds)
        {
            if (userIds == null) return Enumerable.Empty<UserJurisdiction>();

            var idList = userIds.ToList();
            return await _db.UserJurisdictions
                .AsNoTracking()
                .Where(x => idList.Contains(x.UserId))
                .Include(x => x.Jurisdiction)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UpdateUserGeographyAsync(string userId, IEnumerable<int> areaIds, IEnumerable<int> regionIds, IEnumerable<int> jurisdictionIds)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or whitespace.", nameof(userId));
            }

            _log.LogInformation("Updating geographic profile linkages atomically for user: {UserId}", userId);

            // Using explicit db transaction to guarantee atomicity across the three tables
            using (var transaction = await _db.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    await SyncUserAreasAsync(userId, areaIds).ConfigureAwait(false);
                    await SyncUserRegionsAsync(userId, regionIds).ConfigureAwait(false);
                    await SyncUserJurisdictionsAsync(userId, jurisdictionIds).ConfigureAwait(false);

                    await _db.SaveChangesAsync().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Failed to update geographic profile linkages atomically for user: {UserId}. Transaction rolled back.", userId);
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    throw;
                }
            }
        }

        private async Task SyncUserAreasAsync(string userId, IEnumerable<int>? selectedIds)
        {
            var selectedIdSet = (selectedIds ?? Enumerable.Empty<int>()).ToHashSet();
            var currentAssignments = await _db.UserAreas
                .Where(x => x.UserId == userId)
                .ToListAsync()
                .ConfigureAwait(false);

            var assignmentsToRemove = currentAssignments
                .Where(x => !selectedIdSet.Contains(x.AreaId))
                .ToList();

            if (assignmentsToRemove.Count > 0)
            {
                _db.UserAreas.RemoveRange(assignmentsToRemove);
            }

            var currentIds = currentAssignments.Select(x => x.AreaId).ToHashSet();
            var assignmentsToAdd = selectedIdSet
                .Except(currentIds)
                .Select(id => new UserArea { UserId = userId, AreaId = id })
                .ToList();

            if (assignmentsToAdd.Count > 0)
            {
                await _db.UserAreas.AddRangeAsync(assignmentsToAdd).ConfigureAwait(false);
            }
        }

        private async Task SyncUserRegionsAsync(string userId, IEnumerable<int>? selectedIds)
        {
            var selectedIdSet = (selectedIds ?? Enumerable.Empty<int>()).ToHashSet();
            var currentAssignments = await _db.UserRegions
                .Where(x => x.UserId == userId)
                .ToListAsync()
                .ConfigureAwait(false);

            var assignmentsToRemove = currentAssignments
                .Where(x => !selectedIdSet.Contains(x.RegionId))
                .ToList();

            if (assignmentsToRemove.Count > 0)
            {
                _db.UserRegions.RemoveRange(assignmentsToRemove);
            }

            var currentIds = currentAssignments.Select(x => x.RegionId).ToHashSet();
            var assignmentsToAdd = selectedIdSet
                .Except(currentIds)
                .Select(id => new UserRegion { UserId = userId, RegionId = id })
                .ToList();

            if (assignmentsToAdd.Count > 0)
            {
                await _db.UserRegions.AddRangeAsync(assignmentsToAdd).ConfigureAwait(false);
            }
        }

        private async Task SyncUserJurisdictionsAsync(string userId, IEnumerable<int>? selectedIds)
        {
            var selectedIdSet = (selectedIds ?? Enumerable.Empty<int>()).ToHashSet();
            var currentAssignments = await _db.UserJurisdictions
                .Where(x => x.UserId == userId)
                .ToListAsync()
                .ConfigureAwait(false);

            var assignmentsToRemove = currentAssignments
                .Where(x => !selectedIdSet.Contains(x.JurisdictionId))
                .ToList();

            if (assignmentsToRemove.Count > 0)
            {
                _db.UserJurisdictions.RemoveRange(assignmentsToRemove);
            }

            var currentIds = currentAssignments.Select(x => x.JurisdictionId).ToHashSet();
            var assignmentsToAdd = selectedIdSet
                .Except(currentIds)
                .Select(id => new UserJurisdiction { UserId = userId, JurisdictionId = id })
                .ToList();

            if (assignmentsToAdd.Count > 0)
            {
                await _db.UserJurisdictions.AddRangeAsync(assignmentsToAdd).ConfigureAwait(false);
            }
        }
    }
}
