#region license
// Copyright 2024 Utah Departement of Transportation
// for Identity - ATSPM.Identity.Business.Users/UsersService.cs
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
using Identity.Business.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Identity.Business.Users
{
    public class UsersService
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UsersService(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
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
            try {
            await userManager.AddToRolesAsync(user, rolesToAdd);
            await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            } catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
    }
}