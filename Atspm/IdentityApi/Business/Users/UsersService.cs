#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityApi - Identity.Business.Users/UsersService.cs
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
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;

namespace Identity.Business.Users
{
    public class UsersService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ConfigContext configContext;

        public UsersService(UserManager<ApplicationUser> userManager, ConfigContext configContext)
        {
            this.userManager = userManager;
            this.configContext = configContext;
        }

        public async Task updateUserFields(UserDTO model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(model.UserId));
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Agency = model.Agency;
            user.Email = model.Email;
            user.UserName = model.UserName;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new ArgumentException("Failed to update user details");
            }

            var existingRoles = await userManager.GetRolesAsync(user);
            var rolesToAdd = model.Roles.Except(existingRoles).ToList();
            var rolesToRemove = existingRoles.Except(model.Roles).ToList();
            try
            {
                await userManager.AddToRolesAsync(user, rolesToAdd);
                await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

            await SyncUserAreasAsync(user.Id, model.AreaIds);
            await SyncUserRegionsAsync(user.Id, model.RegionIds);
            await SyncUserJurisdictionsAsync(user.Id, model.JurisdictionIds);

            await configContext.SaveChangesAsync();
        }

        private async Task SyncUserAreasAsync(string userId, IEnumerable<int>? selectedIds)
        {
            var selectedIdSet = (selectedIds ?? []).ToHashSet();
            var currentAssignments = await configContext.UserAreas
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var assignmentsToRemove = currentAssignments
                .Where(x => !selectedIdSet.Contains(x.AreaId))
                .ToList();

            if (assignmentsToRemove.Count > 0)
            {
                configContext.UserAreas.RemoveRange(assignmentsToRemove);
            }

            var currentIds = currentAssignments.Select(x => x.AreaId).ToHashSet();
            var assignmentsToAdd = selectedIdSet
                .Except(currentIds)
                .Select(id => new UserArea { UserId = userId, AreaId = id })
                .ToList();

            if (assignmentsToAdd.Count > 0)
            {
                await configContext.UserAreas.AddRangeAsync(assignmentsToAdd);
            }
        }

        private async Task SyncUserRegionsAsync(string userId, IEnumerable<int>? selectedIds)
        {
            var selectedIdSet = (selectedIds ?? []).ToHashSet();
            var currentAssignments = await configContext.UserRegions
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var assignmentsToRemove = currentAssignments
                .Where(x => !selectedIdSet.Contains(x.RegionId))
                .ToList();

            if (assignmentsToRemove.Count > 0)
            {
                configContext.UserRegions.RemoveRange(assignmentsToRemove);
            }

            var currentIds = currentAssignments.Select(x => x.RegionId).ToHashSet();
            var assignmentsToAdd = selectedIdSet
                .Except(currentIds)
                .Select(id => new UserRegion { UserId = userId, RegionId = id })
                .ToList();

            if (assignmentsToAdd.Count > 0)
            {
                await configContext.UserRegions.AddRangeAsync(assignmentsToAdd);
            }
        }

        private async Task SyncUserJurisdictionsAsync(string userId, IEnumerable<int>? selectedIds)
        {
            var selectedIdSet = (selectedIds ?? []).ToHashSet();
            var currentAssignments = await configContext.UserJurisdictions
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var assignmentsToRemove = currentAssignments
                .Where(x => !selectedIdSet.Contains(x.JurisdictionId))
                .ToList();

            if (assignmentsToRemove.Count > 0)
            {
                configContext.UserJurisdictions.RemoveRange(assignmentsToRemove);
            }

            var currentIds = currentAssignments.Select(x => x.JurisdictionId).ToHashSet();
            var assignmentsToAdd = selectedIdSet
                .Except(currentIds)
                .Select(id => new UserJurisdiction { UserId = userId, JurisdictionId = id })
                .ToList();

            if (assignmentsToAdd.Count > 0)
            {
                await configContext.UserJurisdictions.AddRangeAsync(assignmentsToAdd);
            }
        }
    }
}
