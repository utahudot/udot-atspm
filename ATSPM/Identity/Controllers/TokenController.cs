using Identity.Models.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration configuration;

        public TokenController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            this.configuration = configuration;
        }

        [HttpPost("verify/reset")]
        public async Task<IActionResult> VerifyResetToken(VerifyResetTokenViewModel model)
        {
            if (string.IsNullOrEmpty(model.Token))
            {
                return BadRequest(new { message = "Token is required." });
            }

            var user = await _userManager.FindByEmailAsync(model.Username);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Verify the reset token
            var result = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, UserManager<ApplicationUser>.ResetPasswordTokenPurpose, model.Token);

            if (result)
            {
                return Ok(new { message = "OK" });
            }
            else
            {
                return Unauthorized(new { message = "Unauthorized" });
            }
        }

        [Authorize("RequireValidToken")]
        [HttpPost("verify/connect")]
        public async Task<IActionResult> VerifyConnectToken(VerifyConnectTokenViewModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
            {
                return BadRequest(new { message = "Username is required." });
            }

            var user = await _userManager.FindByEmailAsync(model.Username);
            var roles = await _userManager.GetRolesAsync(user);

            if (user != null && roles.Contains("Admin"))
            {
                return Ok("Token verification successful. User has required claims.");
            }
            else
            {
                return Unauthorized("User does not have the required claims.");
            }
        }
    }
}
