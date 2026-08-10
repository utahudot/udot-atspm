#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.Identity/IdentityService.cs
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.LogMessages.Identity;
using Utah.Udot.Atspm.Services.Identity;
using Utah.Udot.Atspm.Services.Identity.Dto;

namespace Utah.Udot.Atspm.Infrastructure.Services.Identity
{
    /// <inheritdoc cref="IIdentityService"/>
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IdentityContext _identityContext;
        private readonly IUserGeographyRepository _geographyRepository;
        private readonly IdentityServiceLogMessages _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityService"/> class.
        /// </summary>
        /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
        /// <param name="identityContext">The Entity Framework Core identity context.</param>
        /// <param name="geographyRepository">The custom geographic links repository.</param>
        /// <param name="logger">The logger instance.</param>
        public IdentityService(
            UserManager<ApplicationUser> userManager,
            IdentityContext identityContext,
            IUserGeographyRepository geographyRepository,
            ILogger<IdentityService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _identityContext = identityContext ?? throw new ArgumentNullException(nameof(identityContext));
            _geographyRepository = geographyRepository ?? throw new ArgumentNullException(nameof(geographyRepository));
            _log = new IdentityServiceLogMessages(logger ?? throw new ArgumentNullException(nameof(logger)));
        }

        /// <inheritdoc/>
        public async Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _log.UserCreationInitiated(request.Email);

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Agency = request.Agency,
                EmailConfirmed = true
            };

            var tempPassword = Guid.NewGuid().ToString("N") + "1!Aa";
            var createResult = await _userManager.CreateAsync(user, tempPassword).ConfigureAwait(false);

            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                _log.UserCreationFailed(request.Email, errors);
                throw new InvalidOperationException($"Failed to create user account: {errors}");
            }

            try
            {
                if (request.Roles != null && request.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, request.Roles).ConfigureAwait(false);
                    if (!roleResult.Succeeded)
                    {
                        var roleErrors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"User created, but role assignment failed: {roleErrors}");
                    }
                }

                await _geographyRepository.UpdateUserGeographyAsync(
                    user.Id,
                    request.AreaIds,
                    request.RegionIds,
                    request.JurisdictionIds
                ).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.UserCreationFailed(request.Email, $"Geographic and role profile sync exception: {ex.Message}");
                await _userManager.DeleteAsync(user).ConfigureAwait(false);
                throw;
            }

            _log.UserCreatedSuccessfully(request.Email, user.Id);

            return new UserResponseDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Agency,
                request.Roles ?? Enumerable.Empty<string>(),
                request.AreaIds ?? Enumerable.Empty<int>(),
                request.RegionIds ?? Enumerable.Empty<int>(),
                request.JurisdictionIds ?? Enumerable.Empty<int>()
            );
        }

        /// <inheritdoc/>
        public async Task<UserResponseDto> GetUserByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
            }

            _log.UserRetrievalById(id);

            var user = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' was not found.");
            }

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            var userIdsList = new List<string> { id };
            var areas = await _geographyRepository.GetAreasByUserIdsAsync(userIdsList).ConfigureAwait(false);
            var regions = await _geographyRepository.GetRegionsByUserIdsAsync(userIdsList).ConfigureAwait(false);
            var jurisdictions = await _geographyRepository.GetJurisdictionsByUserIdsAsync(userIdsList).ConfigureAwait(false);

            return new UserResponseDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.Agency,
                roles,
                areas.Select(x => x.AreaId).ToList(),
                regions.Select(x => x.RegionId).ToList(),
                jurisdictions.Select(x => x.JurisdictionId).ToList()
            );
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            _log.UsersBulkQueryInitiated();

            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .ToListAsync()
                .ConfigureAwait(false);

            var userIds = users.Select(u => u.Id).ToList();

            var userRolesQuery = await _identityContext.UserRoles
                .Join(_identityContext.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => new { ur.UserId, r.Name })
                .ToListAsync()
                .ConfigureAwait(false);

            var rolesGroupedByUser = userRolesQuery
                .GroupBy(ur => ur.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

            var areas = await _geographyRepository.GetAreasByUserIdsAsync(userIds).ConfigureAwait(false);
            var regions = await _geographyRepository.GetRegionsByUserIdsAsync(userIds).ConfigureAwait(false);
            var jurisdictions = await _geographyRepository.GetJurisdictionsByUserIdsAsync(userIds).ConfigureAwait(false);

            var areasGroupedByUser = areas.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => g.Select(x => x.AreaId).ToList());
            var regionsGroupedByUser = regions.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => g.Select(x => x.RegionId).ToList());
            var jurisdictionsGroupedByUser = jurisdictions.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => g.Select(x => x.JurisdictionId).ToList());

            var userResponseDtos = new List<UserResponseDto>();
            foreach (var user in users)
            {
                rolesGroupedByUser.TryGetValue(user.Id, out var roles);
                areasGroupedByUser.TryGetValue(user.Id, out var userAreas);
                regionsGroupedByUser.TryGetValue(user.Id, out var userRegions);
                jurisdictionsGroupedByUser.TryGetValue(user.Id, out var userJurisdictions);

                userResponseDtos.Add(new UserResponseDto(
                    user.Id,
                    user.Email ?? string.Empty,
                    user.FirstName,
                    user.LastName,
                    user.Agency,
                    roles ?? Enumerable.Empty<string>(),
                    userAreas ?? Enumerable.Empty<int>(),
                    userRegions ?? Enumerable.Empty<int>(),
                    userJurisdictions ?? Enumerable.Empty<int>()
                ));
            }

            _log.UsersBulkQueryCompleted(userResponseDtos.Count);

            return userResponseDtos;
        }

        /// <inheritdoc/>
        public async Task UpdateUserAsync(string id, UpdateUserRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _log.UserUpdateInitiated(request.Email, id);

            var user = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' was not found.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Agency = request.Agency;
            user.Email = request.Email;
            user.UserName = request.Email;

            var updateResult = await _userManager.UpdateAsync(user).ConfigureAwait(false);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update identity user details: {errors}");
            }

            var currentRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            var rolesToAdd = request.Roles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(request.Roles).ToList();

            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd).ConfigureAwait(false);
            }
            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove).ConfigureAwait(false);
            }

            await _geographyRepository.UpdateUserGeographyAsync(
                user.Id,
                request.AreaIds,
                request.RegionIds,
                request.JurisdictionIds
            ).ConfigureAwait(false);

            _log.UserUpdateCompleted(id);
        }

        /// <inheritdoc/>
        public async Task DeleteUserAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
            }

            _log.UserDeletionInitiated(id);

            var user = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' was not found.");
            }

            await _geographyRepository.UpdateUserGeographyAsync(
                user.Id,
                Enumerable.Empty<int>(),
                Enumerable.Empty<int>(),
                Enumerable.Empty<int>()
            ).ConfigureAwait(false);

            var deleteResult = await _userManager.DeleteAsync(user).ConfigureAwait(false);
            if (!deleteResult.Succeeded)
            {
                var errors = string.Join("; ", deleteResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to delete user account: {errors}");
            }

            _log.UserDeletionCompleted(id);
        }
    }
}
