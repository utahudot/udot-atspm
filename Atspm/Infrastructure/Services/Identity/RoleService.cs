#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.Identity/RoleService.cs
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
using System.Security.Claims;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Infrastructure.LogMessages.Identity;
using Utah.Udot.Atspm.Services.Identity;
using Utah.Udot.Atspm.Services.Identity.Dto;

namespace Utah.Udot.Atspm.Infrastructure.Services.Identity
{
    /// <inheritdoc cref="IRoleService"/>
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly RoleServiceLogMessages _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleService"/> class.
        /// </summary>
        /// <param name="roleManager">The ASP.NET Core Identity role manager.</param>
        /// <param name="logger">The logger instance.</param>
        public RoleService(
            RoleManager<IdentityRole> roleManager,
            ILogger<RoleService> logger)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _log = new RoleServiceLogMessages(logger ?? throw new ArgumentNullException(nameof(logger)));
        }

        /// <inheritdoc/>
        public async Task CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role name cannot be null or empty.", nameof(roleName));
            }

            _log.RoleCreationInitiated(roleName);

            var existingRole = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
            if (existingRole != null)
            {
                throw new InvalidOperationException($"Role '{roleName}' already exists.");
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName)).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create role: {errors}");
            }

            _log.RoleCreatedSuccessfully(roleName);
        }

        /// <inheritdoc/>
        public async Task DeleteRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role name cannot be null or empty.", nameof(roleName));
            }

            _log.RoleDeletionInitiated(roleName);

            var role = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role '{roleName}' was not found.");
            }

            var result = await _roleManager.DeleteAsync(role).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to delete role: {errors}");
            }

            _log.RoleDeletedSuccessfully(roleName);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .ToListAsync()
                .ConfigureAwait(false);

            var roleResponseDtos = new List<RoleResponseDto>();
            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role).ConfigureAwait(false);
                roleResponseDtos.Add(new RoleResponseDto(
                    role.Name ?? string.Empty,
                    claims.Select(c => c.Value).ToList()
                ));
            }

            return roleResponseDtos;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetClaimsForRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role name cannot be null or empty.", nameof(roleName));
            }

            var role = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role '{roleName}' was not found.");
            }

            var claims = await _roleManager.GetClaimsAsync(role).ConfigureAwait(false);
            return claims.Select(c => c.Value).ToList();
        }

        /// <inheritdoc/>
        public async Task SyncClaimsToRoleAsync(string roleName, IEnumerable<string> permissions)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role name cannot be null or empty.", nameof(roleName));
            }
            if (permissions == null)
            {
                throw new ArgumentNullException(nameof(permissions));
            }

            _log.RoleSyncClaimsInitiated(roleName);

            var role = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role '{roleName}' was not found.");
            }

            var existingClaims = await _roleManager.GetClaimsAsync(role).ConfigureAwait(false);
            var existingValues = existingClaims.Select(c => c.Value).ToHashSet();
            var targetValues = permissions.ToHashSet();

            var claimsToAdd = targetValues.Except(existingValues).ToList();
            var claimsToRemove = existingClaims.Where(c => !targetValues.Contains(c.Value)).ToList();

            foreach (var claim in claimsToRemove)
            {
                var result = await _roleManager.RemoveClaimAsync(role, claim).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to remove stale claim '{claim.Value}' from role '{roleName}'.");
                }
            }

            foreach (var permission in claimsToAdd)
            {
                var newClaim = new Claim(AtspmAuthorization.RoleClaimType, permission);
                var result = await _roleManager.AddClaimAsync(role, newClaim).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to assign claim '{permission}' to role '{roleName}'.");
                }
            }

            _log.RoleSyncClaimsCompleted(roleName, claimsToAdd.Count, claimsToRemove.Count);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetAllSystemPermissionsAsync()
        {
            _log.SystemPermissionsQueried();

            return await Task.FromResult(Enum.GetValues(typeof(Utah.Udot.Atspm.Enums.ClaimTypes))
                .Cast<Enum>()
                .Select(e => e.GetDisplayName())
                .ToList()).ConfigureAwait(false);
        }
    }
}
