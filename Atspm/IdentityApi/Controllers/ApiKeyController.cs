#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityApi - Utah.Udot.ATSPM.IdentityApi.Controllers/ApiKeyController.cs
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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Infrastructure.Attributes;
using Utah.Udot.Atspm.Services.ApiKeys;

namespace Utah.Udot.ATSPM.IdentityApi.Controllers
{
    /// <summary>
    /// Provides endpoints for managing API keys, including creation, retrieval, and revocation.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyController"/> class.
        /// </summary>
        /// <param name="apiKeyService">The API key business layer service.</param>
        public ApiKeyController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        /// <summary>
        /// Generates and stores a new API key for the authenticated user (or on behalf of another user if requested by an Admin).
        /// </summary>
        /// <param name="dto">The details for the new API key.</param>
        /// <returns>An IActionResult containing the raw API key and instructions.</returns>
        /// <response code="200">Returns the generated raw key. Note: This is only shown once.</response>
        /// <response code="400">If the request payload was invalid or unparseable.</response>
        /// <response code="401">Unauthorized if the user identity cannot be resolved.</response>
        /// <response code="403">Forbidden if the user attempts to grant claims they do not possess.</response>
        /// <response code="404">If the specified target user was not found.</response>
        [AuthorizePermission(AtspmAuthorization.Permissions.ApiKeysCreate, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiKeyCreatedResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateApiKeyDto dto)
        {
            try
            {
                var result = await _apiKeyService.CreateKeyAsync(dto, User);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid Request"
                );
            }
            catch (KeyNotFoundException ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "User Not Found"
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Insufficient Permissions"
                );
            }
        }

        /// <summary>
        /// Retrieves all active, non-revoked API keys belonging to the authenticated user.
        /// </summary>
        /// <returns>A list of API key summaries.</returns>
        /// <response code="200">Returns the list of keys associated with the user.</response>
        /// <response code="401">Unauthorized if the user identity cannot be resolved.</response>
        /// <response code="403">Forbidden if the user lacks the required view permission.</response>
        [AuthorizePermission(AtspmAuthorization.Permissions.ApiKeysView, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("my-keys")]
        [ProducesResponseType(typeof(IEnumerable<ApiKeySummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyKeys()
        {
            try
            {
                var keys = await _apiKeyService.GetKeysForUserAsync(User);
                return Ok(keys);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );
            }
        }

        /// <summary>
        /// Retrieves all active, non-revoked API keys in the system.
        /// </summary>
        /// <returns>A list of all system API keys with details.</returns>
        /// <response code="200">Returns the complete list of system keys.</response>
        /// <response code="401">Unauthorized if the user identity cannot be resolved.</response>
        /// <response code="403">Forbidden if the user lacks global view permissions.</response>
        [Authorize(Policy = AtspmAuthorization.Roles.Admin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("all-keys")]
        [ProducesResponseType(typeof(IEnumerable<ApiKeyDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllKeys()
        {
            var keys = await _apiKeyService.GetAllSystemKeysAsync(User);
            return Ok(keys);
        }

        /// <summary>
        /// Marks a specific API key as revoked to prevent further use.
        /// </summary>
        /// <param name="id">The unique identifier of the API key to revoke.</param>
        /// <returns>A status message regarding the revocation.</returns>
        /// <response code="200">The key was successfully revoked or was already revoked.</response>
        /// <response code="401">Unauthorized if the user identity cannot be resolved.</response>
        /// <response code="403">Forbidden if the user lacks the required revoke permission.</response>
        /// <response code="404">The key was not found or the user does not own it.</response>
        [AuthorizePermission(AtspmAuthorization.Permissions.ApiKeysRevoke, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("revoke/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Revoke(int id)
        {
            try
            {
                var wasRevoked = await _apiKeyService.RevokeKeyAsync(id, User);
                if (!wasRevoked)
                {
                    return Ok(new { Message = "API Key was already revoked." });
                }

                return Ok(new { Message = "API Key has been revoked." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );
            }
            catch (KeyNotFoundException ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Key Not Found"
                );
            }
        }
    }
}
