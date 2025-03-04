#region license
// Copyright 2025 Utah Departement of Transportation
// for IdentityApi - Identity.Controllers/RolesController.cs
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

using Asp.Versioning;
using Identity.Business.Claims;
using Identity.Business.Roles;
using Identity.Models.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.ATSPM.IdentityApi.Controllers;

namespace Identity.Controllers
{
    //[Authorize()]
    [ApiVersion("1.0")]
    public class RolesController : IdentityControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ClaimsService claimsService)
        {
            this.roleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Policy = "CanViewRoles")]
        public async Task<IActionResult> GetRolesAsync()
        {
            var roles = roleManager.Roles.ToList();
            var result = new List<RolesResult>();

            foreach (var role in roles)
            {
                var claims = await roleManager.GetClaimsAsync(role);
                result.Add(new RolesResult { Role = role.Name, Claims = claims.Select(c => c.Value).ToList() });
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "CanEditRoles")]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = new IdentityRole { Name = model.RoleName };

            var result = await roleManager.CreateAsync(role);
            if (result != null && result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result?.Errors);
            }
        }

        [HttpDelete("{roleName}")]
        [Authorize(Policy = "CanDeleteRoles")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound();
            }

            var result = await roleManager.DeleteAsync(role);
            if (result != null && result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result?.Errors);
            }
        }
    }
}