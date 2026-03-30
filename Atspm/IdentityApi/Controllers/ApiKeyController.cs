using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;

namespace Utah.Udot.ATSPM.IdentityApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ApiKeyController : ControllerBase
    {
        private readonly IdentityContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApiKeyController(IdentityContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateApiKeyDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // 1. Generate the random key
            var (rawKey, hash) = ApiKeyGenerator.CreateKey();

            // 2. Prepare the entity
            var apiKey = new ApiKey
            {
                Name = dto.Name,
                KeyHash = hash,
                OwnerId = userId,
                ExpiresAt = dto.ExpiresAt,
                IsRevoked = false,
                Claims = dto.Roles.Select(r => new ApiKeyClaim
                {
                    Type = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    Value = r
                }).ToList()
            };

            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();

            // 3. Return the RAW key only here.
            return Ok(new { Key = rawKey, Message = "Copy this now! It will not be shown again." });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("my-keys")]
        public async Task<IActionResult> GetMyKeys()
        {
            var userId = _userManager.GetUserId(User);
            var keys = await _context.ApiKeys
                .Where(k => k.OwnerId == userId && !k.IsRevoked)
                .Select(k => new { k.Id, k.Name, k.CreatedAt, k.ExpiresAt })
                .ToListAsync();

            return Ok(keys);
        }
    }

    public class CreateApiKeyDto
    {
        public string Name { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<string> Roles { get; set; }
    }
}
