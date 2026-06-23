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
using System.Reflection;
using System.Security.Claims;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.Attributes;
using Utah.Udot.ATSPM.IdentityApi.Dto;
using Utah.Udot.NetStandardToolkit.Common;

namespace Utah.Udot.ATSPM.IdentityApi.Controllers
{
    /// <summary>
    /// Provides endpoints for managing API keys, including creation, retrieval, and revocation.
    /// </summary>
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ApiKeyController : ControllerBase
    {
        private static readonly HashSet<string> AllPermissions = typeof(AtspmAuthorization.Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .Select(f => f.GetRawConstantValue()?.ToString())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

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
        /// Generates and stores a new API key for the authenticated user.
        /// </summary>
        /// <param name="dto">The details for the new API key.</param>
        /// <returns>An IActionResult containing the raw API key.</returns>
        /// <response code="200">Returns the generated raw key. Note: This is only shown once.</response>
        /// <response code="401">Unauthorized if the user identity cannot be resolved.</response>
        /// <response code="403">Forbidden if the user attempts to grant claims they do not possess.</response>
        [AuthorizePermission(AtspmAuthorization.Permissions.ApiKeysCreate, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
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

            var validationResult = ValidateCreateRequest(dto);
            if (validationResult != null) return validationResult;

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var requestedClaims = dto.Claims
                .Select(c => c.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var isGlobalAdmin = IsGlobalAdmin(User);

            if (!isGlobalAdmin)
            {
                foreach (var requestedClaim in requestedClaims)
                {
                    if (!User.HasClaim(c => c.Type == AtspmAuthorization.RoleClaimType && string.Equals(c.Value, requestedClaim, StringComparison.OrdinalIgnoreCase)))
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
                Name = dto.Name.Trim(),
                KeyHash = hash,
                OwnerId = userId,
                ExpiresAt = dto.ExpiresAt,
                IsRevoked = false,
                Claims = requestedClaims.Select(r => new ApiKeyClaim
                {
                    Type = AtspmAuthorization.RoleClaimType,
                    Value = r
                }).ToList()
            };

            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Key = rawKey,
                Message = "Copy this key now. For security reasons, it cannot be retrieved again."
            });
        }

        /// <summary>
        /// Retrieves API keys visible to the authenticated user.
        /// </summary>
        /// <returns>A list of API key metadata.</returns>
        /// <response code="200">Returns the list of keys associated with the user.</response>
        /// <response code="401">Unauthorized if the user identity cannot be resolved.</response>
        /// <response code="403">Forbidden if the user lacks the required view permission.</response>
        [AuthorizePermission(AtspmAuthorization.Permissions.ApiKeysView, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("my-keys")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyKeys()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var isGlobalAdmin = IsGlobalAdmin(User);

            var query = _context.ApiKeys
                .Include(k => k.Claims)
                .AsNoTracking();

            if (!isGlobalAdmin)
            {
                query = query.Where(k => k.OwnerId == userId);
            }

            var keys = await query
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();

            var ownerIds = keys
                .Select(k => k.OwnerId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var owners = await _context.Users
                .Where(u => ownerIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName
                })
                .ToListAsync();

            var ownerById = owners.ToDictionary(u => u.Id);

            var result = keys.Select(k =>
            {
                ownerById.TryGetValue(k.OwnerId, out var owner);
                var ownerName = string.Join(" ", new[] { owner?.FirstName, owner?.LastName }
                    .Where(part => !string.IsNullOrWhiteSpace(part)));

                return new ApiKeyMetadataDto
                {
                    Id = k.Id,
                    Name = k.Name,
                    OwnerId = k.OwnerId,
                    OwnerEmail = owner?.Email ?? string.Empty,
                    OwnerName = ownerName,
                    CreatedAt = k.CreatedAt,
                    ExpiresAt = k.ExpiresAt,
                    IsRevoked = k.IsRevoked,
                    Claims = k.Claims.Select(c => c.Value).OrderBy(c => c).ToList()
                };
            }).ToList();

            return Ok(result);
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

            var query = _context.ApiKeys.AsQueryable();
            if (!IsGlobalAdmin(User))
            {
                query = query.Where(k => k.OwnerId == userId);
            }

            var apiKey = await query.FirstOrDefaultAsync(k => k.Id == id);

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

        private static bool IsGlobalAdmin(ClaimsPrincipal user)
        {
            return user.HasClaim(c => c.Type == AtspmAuthorization.RoleClaimType && c.Value == AtspmAuthorization.Roles.Admin);
        }

        private IActionResult? ValidateCreateRequest(CreateApiKeyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return Problem(
                    detail: "API key name is required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid API Key Name"
                );
            }

            if (dto.Name.Length > 200)
            {
                return Problem(
                    detail: "API key name must be 200 characters or fewer.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid API Key Name"
                );
            }

            if (dto.Claims == null || dto.Claims.Count == 0 || dto.Claims.Any(string.IsNullOrWhiteSpace))
            {
                return Problem(
                    detail: "At least one non-empty permission is required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid API Key Claims"
                );
            }

            var requestedClaims = dto.Claims
                .Select(c => c.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var forbiddenClaims = requestedClaims
                .Where(IsForbiddenApiKeyClaim)
                .ToList();

            if (forbiddenClaims.Any())
            {
                return Problem(
                    detail: $"API keys cannot be granted these permissions: {string.Join(", ", forbiddenClaims)}.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Forbidden API Key Claims"
                );
            }

            var unknownClaims = requestedClaims
                .Where(c => !AllPermissions.Contains(c))
                .ToList();

            if (unknownClaims.Any())
            {
                return Problem(
                    detail: $"Unknown API key permissions: {string.Join(", ", unknownClaims)}.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid API Key Claims"
                );
            }

            return null;
        }

        private static bool IsForbiddenApiKeyClaim(string claim)
        {
            return string.Equals(claim, AtspmAuthorization.Permissions.Admin, StringComparison.OrdinalIgnoreCase)
                || AtspmAuthorization.IsApiKeyPermission(claim);
        }
    }
}
