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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Services.Identity;
using Utah.Udot.Atspm.Services.Identity.Dto;

namespace Utah.Udot.ATSPM.IdentityApi.Controllers.v2
{
    /// <summary>
    /// Handles roles and role-claims mapping management.
    /// </summary>
    [ApiVersion("2.0")]
    [Authorize(Roles = "Admin")]
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
        [ProducesResponseType(typeof(IDictionary<string, List<string>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSystemPermissions()
        {
            var permissions = await _roleService.GetSystemPermissionsAsync();
            return Ok(permissions);
        }

        /// <summary>
        /// Retrieves the list of all defined roles in the system.
        /// </summary>
        /// <returns>A collection of role definitions.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleService.GetRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Creates a brand new system role.
        /// </summary>
        /// <param name="model">The specification of the role to create.</param>
        /// <returns>The newly created role on success.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto model)
        {
            if (model == null)
            {
                return BadRequest("Role definition cannot be null.");
            }

            var result = await _roleService.CreateRoleAsync(model);
            if (result != null)
            {
                return Ok(result);
            }

            return BadRequest("Failed to create role.");
        }

        /// <summary>
        /// Deletes an existing system role.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <returns>A status indicating success or failure.</returns>
        [HttpDelete("{roleName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteRole([FromRoute] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest("Role name must be specified.");
            }

            var result = await _roleService.DeleteRoleAsync(roleName);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Retrieves the exact permissions mapped to a specific system role.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>A collection of claim structures representing the active permissions.</returns>
        [HttpGet("{roleName}/claims")]
        [ProducesResponseType(typeof(IEnumerable<ClaimDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRoleClaims([FromRoute] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest("Role name is required.");
            }

            var claims = await _roleService.GetRoleClaimsAsync(roleName);
            return Ok(claims);
        }

        /// <summary>
        /// Updates the set of permission claims assigned to a specific system role.
        /// </summary>
        /// <param name="roleName">The name of the role to modify.</param>
        /// <param name="claims">The updated set of permission claims.</param>
        /// <returns>A confirmation of successful update.</returns>
        [HttpPost("{roleName}/claims")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRoleClaims([FromRoute] string roleName, [FromBody] IEnumerable<ClaimDto> claims)
        {
            if (string.IsNullOrWhiteSpace(roleName) || claims == null)
            {
                return BadRequest("Role name and claim permissions list are required.");
            }

            var result = await _roleService.UpdateRoleClaimsAsync(roleName, claims);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result);
        }
    }
}
