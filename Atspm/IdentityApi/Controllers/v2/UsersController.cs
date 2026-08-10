#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityApi - Identity.Controllers/UsersController.cs
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
    /// Handles user profiles, role assignments, registrations, and administrative user account operations.
    /// </summary>
    [ApiVersion("2.0")]
    [Produces("application/json")]
    public class UsersController : IdentityControllerBase
    {
        private readonly IIdentityService _identityService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="identityService">The identity management service.</param>
        public UsersController(IIdentityService identityService)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        }

        /// <summary>
        /// Registers a brand new user account within the ATSPM identity store.
        /// </summary>
        /// <param name="model">The registration payload details.</param>
        /// <returns>A confirmation containing the created user's account details.</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] CreateUserRequestDto model)
        {
            if (model == null)
            {
                return BadRequest("Registration payload cannot be null.");
            }

            var result = await _identityService.CreateUserAsync(model);
            if (result != null)
            {
                return Ok(result);
            }

            return BadRequest("User registration failed.");
        }

        /// <summary>
        /// Fetches the profile details of the currently authenticated user.
        /// </summary>
        /// <returns>The authenticated user's profile and regional constraints.</returns>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var profile = await _identityService.GetUserByIdAsync(userId);
            if (profile == null)
            {
                return NotFound("User profile not found.");
            }

            return Ok(profile);
        }

        /// <summary>
        /// Updates the profile data of the currently authenticated user.
        /// </summary>
        /// <param name="model">The updated profile details.</param>
        /// <returns>A confirmation of successful update.</returns>
        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequestDto model)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            if (model == null)
            {
                return BadRequest("Profile update content cannot be null.");
            }

            await _identityService.UpdateUserAsync(userId, model);
            return Ok();
        }

        /// <summary>
        /// Fetches all registered users within the system (Admin only).
        /// </summary>
        /// <returns>The comprehensive list of system users.</returns>
        [HttpGet]
        [AuthorizePermission(AtspmAuthorization.Permissions.UsersView, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _identityService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Administers a full update of another user's account properties (Admin only).
        /// </summary>
        /// <param name="id">The identifier of the user to update.</param>
        /// <param name="model">The complete updated account payload.</param>
        /// <returns>An empty confirmation of successful update.</returns>
        [HttpPut("{id}")]
        [AuthorizePermission(AtspmAuthorization.Permissions.UsersEdit, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UpdateUserRequestDto model)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null)
            {
                return BadRequest("Invalid route parameter or empty user model identity.");
            }

            await _identityService.UpdateUserAsync(id, model);
            return Ok();
        }

        /// <summary>
        /// Deletes a user account from the identity repository (Admin only).
        /// </summary>
        /// <param name="id">The identifier of the user to delete.</param>
        /// <returns>A status code confirming deletion or a bad request.</returns>
        [HttpDelete("{id}")]
        [AuthorizePermission(AtspmAuthorization.Permissions.UsersDelete, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("User ID must be specified.");
            }

            await _identityService.DeleteUserAsync(id);
            return Ok();
        }
    }
}
