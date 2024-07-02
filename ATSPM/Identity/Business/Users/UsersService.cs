using ATSPM.Data;
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