#region license
// Copyright 2026 Utah Departement of Transportation
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Infrastructure.Attributes;
using Utah.Udot.Atspm.Services.Identity;
using Utah.Udot.Atspm.Services.Identity.Dto;

namespace Utah.Udot.ATSPM.IdentityApi.Controllers.v2
{
    /// <summary>
    /// Handles roles and role-claims mapping management.
    /// </summary>
    [ApiVersion("2.0")]
    [Produces("application/json")]
    public class RolesController : IdentityControllerBase
    {
        private readonly IRoleService _roleService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RolesController"/> class.
        /// </summary>
        /// <param name="roleService">The roles and claims management service.</param>
        public RolesController(IRoleService roleService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        /// <summary>
        /// Retrieves the list of all available system permissions configured in ATSPM.
        /// </summary>
        /// <returns>A dictionary grouping category scopes to lists of claim permissions.</returns>
        [HttpGet("permissions")]
        [AuthorizePermission(AtspmAuthorization.Permissions.RolesView, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSystemPermissions()
        {
            var permissions = await _roleService.GetAllSystemPermissionsAsync();
            return Ok(permissions);
        }

        /// <summary>
        /// Retrieves the list of all defined roles in the system.
        /// </summary>
        /// <returns>A collection of role definitions.</returns>
        [HttpGet]
        [AuthorizePermission(AtspmAuthorization.Permissions.RolesView, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(IEnumerable<RoleResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Creates a brand new system role.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        /// <returns>An action result indicating success.</returns>
        [HttpPost("{roleName}")]
        [AuthorizePermission(AtspmAuthorization.Permissions.RolesEdit, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole([FromRoute] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest("Role name cannot be empty.");
            }

            await _roleService.CreateRoleAsync(roleName);
            return Ok();
        }

        /// <summary>
        /// Deletes an existing system role.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <returns>A status indicating success or failure.</returns>
        [HttpDelete("{roleName}")]
        [AuthorizePermission(AtspmAuthorization.Permissions.RolesDelete, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteRole([FromRoute] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest("Role name must be specified.");
            }

            await _roleService.DeleteRoleAsync(roleName);
            return Ok();
        }

        /// <summary>
        /// Retrieves the exact permissions mapped to a specific system role.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>A collection of claim structures representing the active permissions.</returns>
        [HttpGet("{roleName}/claims")]
        [AuthorizePermission(AtspmAuthorization.Permissions.RolesView, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRoleClaims([FromRoute] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest("Role name is required.");
            }

            var claims = await _roleService.GetClaimsForRoleAsync(roleName);
            return Ok(claims);
        }

        /// <summary>
        /// Updates the set of permission claims assigned to a specific system role.
        /// </summary>
        /// <param name="roleName">The name of the role to modify.</param>
        /// <param name="claims">The updated set of permission claims.</param>
        /// <returns>A confirmation of successful update.</returns>
        [HttpPost("{roleName}/claims")]
        [AuthorizePermission(AtspmAuthorization.Permissions.RolesEdit, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRoleClaims([FromRoute] string roleName, [FromBody] IEnumerable<string> claims)
        {
            if (string.IsNullOrWhiteSpace(roleName) || claims == null)
            {
                return BadRequest("Role name and claim permissions list are required.");
            }

            await _roleService.SyncClaimsToRoleAsync(roleName, claims);
            return Ok();
        }
    }
}
