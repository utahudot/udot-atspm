using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.ATSPM.IdentityApi.Dto;

namespace Utah.Udot.ATSPM.IdentityApi.Controllers
{
    /// <summary>
    /// Provides endpoints for managing API keys, including creation, retrieval, and revocation.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class ApiKeyController : ControllerBase
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
        /// Generates and stores a new API key for the authenticated user.
        /// </summary>
        /// <param name="dto">The details for the new API key.</param>
        /// <returns>An IActionResult containing the raw API key.</returns>
        /// <response code="200">Returns the generated raw key. Note: This is only shown once.</response>
        /// <response code="401">Unauthorized if the user identity cannot be resolved.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "CanCreateApiKeys")]
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateApiKeyDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var isGlobalAdmin = User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin");

            if (!isGlobalAdmin)
            {
                foreach (var requestedRole in dto.Roles)
                {
                    if (!User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == requestedRole))
                    {
                        return StatusCode(StatusCodes.Status403Forbidden,
                            $"You cannot grant the permission '{requestedRole}' because you do not possess it.");
                    }
                }
            }

            var (rawKey, hash) = ApiKeyGenerator.CreateKey();

            var apiKey = new ApiKey
            {
                Name = dto.Name,
                KeyHash = hash,
                OwnerId = userId,
                ExpiresAt = dto.ExpiresAt,
                IsRevoked = false,
                Claims = dto.Roles.Select(r => new ApiKeyClaim
                {
                    Type = ClaimTypes.Role,
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
        /// Retrieves all active, non-revoked API keys belonging to the authenticated user.
        /// </summary>
        /// <returns>A list of API key metadata.</returns>
        /// <response code="200">Returns the list of keys associated with the user.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("my-keys")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyKeys()
        {
            var userId = _userManager.GetUserId(User);
            var keys = await _context.ApiKeys
                .Where(k => k.OwnerId == userId && !k.IsRevoked)
                .Select(k => new { k.Id, k.Name, k.CreatedAt, k.ExpiresAt })
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
        /// <response code="404">The key was not found or the user does not own it.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("revoke/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Revoke(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.Id == id && k.OwnerId == userId);

            if (apiKey == null)
            {
                return NotFound("API Key not found or you do not have permission to revoke it.");
            }

            apiKey.IsRevoked = true;

            await _context.SaveChangesAsync();

            return Ok(new { Message = $"API Key '{apiKey.Name}' has been revoked." });
        }
    }
}
