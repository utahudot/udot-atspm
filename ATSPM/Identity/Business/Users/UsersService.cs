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

        public async Task AssignRolesToUser(string userName, IEnumerable<string> roleNames)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userName));
            }

            var existingRoles = await userManager.GetRolesAsync(user);
            var rolesToAdd = roleNames.Except(existingRoles).ToList();
            var rolesToRemove = existingRoles.Except(roleNames).ToList();
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