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

using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.Attributes;
using Utah.Udot.ATSPM.IdentityApi.Attributes;
using Utah.Udot.ATSPM.IdentityApi.Dto;
using Utah.Udot.NetStandardToolkit.Common;

namespace Utah.Udot.ATSPM.IdentityApi.Controllers
{
    /// <summary>
    /// Provides endpoints for managing API keys, including creation, retrieval, and revocation.
    /// </summary>
    [Authorize]
    [RejectApiKeyAuthentication]
    [ApiVersion("1.0")]
    public class ApiKeyController : IdentityControllerBase
    {
        private readonly IdentityContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyController"/> class.
        /// </summary>
        /// <param name="context">The database context for identity and API key data.</param>
        /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
        public ApiKeyController(IdentityContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Generates and stores a new API key for the authenticated user or on behalf of a specified user.
        /// </summary>
        /// <param name="dto">The details for the new API key.</param>
        /// <returns>An <see cref="ApiKeyCreatedResponseDto"/> containing the raw API key and metadata.</returns>
        /// <response code="200">Returns the generated raw key and metadata. Note: The plaintext key is only shown once.</response>
        /// <response code="400">Bad request if the request body is invalid.</response>
        /// <response code="401">Unauthorized if the user identity cannot be resolved.</response>
        /// <response code="403">Forbidden if the user attempts to grant claims they do not possess.</response>
        /// <response code="404">Not found if the specified target user does not exist.</response>
        [AuthorizePermission(AtspmAuthorization.Permissions.ApiKeysCreate, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiKeyCreatedResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateApiKeyDto dto)
        {
            if (dto == null)
            {
                return Problem(
                    detail: "The request body could not be parsed. Check your JSON format and date strings.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid Request Body"
                );
            }

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            string ownerId;
            if (dto.UserId.HasValue)
            {
                ownerId = dto.UserId.Value.ToString();
                var targetUser = await _userManager.FindByIdAsync(ownerId);
                if (targetUser == null)
                {
                    return Problem(
                        detail: $"Target user with ID '{ownerId}' was not found.",
                        statusCode: StatusCodes.Status404NotFound,
                        title: "User Not Found"
                    );
                }
            }
            else
            {
                ownerId = currentUserId;
            }

            var requestedClaims = dto.Claims?.ToList() ?? new List<string>();
            var isGlobalAdmin = User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin");

            if (!isGlobalAdmin)
            {
                foreach (var requestedClaim in requestedClaims)
                {
                    if (!User.HasClaim(c => c.Type == AtspmAuthorization.RoleClaimType && c.Value == requestedClaim))
                    {
                        return Problem(
                            detail: $"You cannot grant the permission '{requestedClaim}' because you do not possess it.",
                            statusCode: StatusCodes.Status403Forbidden,
                            title: "Insufficient Permissions"
                        );
                    }
                }
            }

            var (rawKey, hash) = ApiKeyGenerator.CreateKey();

            var apiKey = new ApiKey
            {
                Name = dto.Name,
                KeyHash = hash,
                OwnerId = ownerId,
                ExpiresAt = dto.ExpiresAt,
                IsRevoked = false,
                Claims = requestedClaims.Select(r => new ApiKeyClaim
                {
                    Type = ClaimTypes.Role,
                    Value = r
                }).ToList()
            };

            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();

            return Ok(new ApiKeyCreatedResponseDto(
                apiKey.Name,
                rawKey,
                apiKey.CreatedAt,
                apiKey.ExpiresAt
            ));
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
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var keys = await _context.ApiKeys
                .Where(k => k.OwnerId == userId && !k.IsRevoked)
                .Select(k => new ApiKeySummaryDto(k.Id, k.Name, k.CreatedAt, k.ExpiresAt))
                .ToListAsync();

            return Ok(keys);
        }

        /// <summary>
        /// Retrieves all active, non-revoked API keys in the system.
        /// </summary>
        /// <returns>A list of all API key details, including assigned claims.</returns>
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
            var keys = await _context.ApiKeys
                .Where(k => !k.IsRevoked)
                .Select(k => new ApiKeyDetailDto(
                    k.Id,
                    k.Name,
                    k.OwnerId,
                    k.CreatedAt,
                    k.ExpiresAt,
                    k.Claims.Select(c => c.Value)
                ))
                .ToListAsync();

            return Ok(keys);
        }

        /// <summary>
        /// Marks a specific API key as revoked to prevent further use.
        /// </summary>
        /// <param name="id">The unique identifier of the API key to revoke.</param>
        /// <returns>A status message regarding the revocation.</returns>
        /// <response code="200">The key was successfully revoked.</response>
        /// <response code="401">Unauthorized if the user identity cannot be resolved.</response>
        /// <response code="403">Forbidden if the user lacks the required revoke permission.</response>
        /// <response code="404">The key was not found or the user does not own it.</response>
        /// <response code="500">Internal server error if the database update failed.</response>
        [AuthorizePermission(AtspmAuthorization.Permissions.ApiKeysRevoke, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("revoke/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Revoke(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var isGlobalAdmin = User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin");

            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.Id == id && (isGlobalAdmin || k.OwnerId == userId));

            if (apiKey == null)
            {
                return Problem(
                    detail: $"API Key with ID {id} not found or access denied.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Key Not Found"
                );
            }

            if (apiKey.IsRevoked)
            {
                return Ok(new { Message = $"API Key '{apiKey.Name}' was already revoked." });
            }

            apiKey.IsRevoked = true;
            _context.Entry(apiKey).State = EntityState.Modified;

            var rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected == 0)
            {
                return Problem(
                    detail: "The database confirmed 0 rows were updated. Check if the key still exists in Postgres.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Database Update Failure"
                );
            }

            return Ok(new { Message = $"API Key '{apiKey.Name}' has been revoked." });
        }
    }
}
