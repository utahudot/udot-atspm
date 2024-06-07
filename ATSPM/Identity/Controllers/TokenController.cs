using Identity.Business.Tokens;
using Identity.Models.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;

        public TokenController(UserManager<ApplicationUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("verify/reset")]
        public async Task<IActionResult> VerifyResetToken(VerifyResetTokenViewModel model)
        {
            if (string.IsNullOrEmpty(model.Token))
            {
                return BadRequest(new { Message = "Token is required." });
            }

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                return Unauthorized(new { Message = "User not found." });
            }

            var resetToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            // Verify the reset token
            var result = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, UserManager<ApplicationUser>.ResetPasswordTokenPurpose, resetToken);

            if (result)
            {
                var token = await _tokenService.GenerateJwtTokenAsync(user);
                return Ok(new { Token = token, Message = "OK" });
            }
            else
            {
                return Unauthorized(new { Message = "Unauthorized" });
            }
        }

        [HttpPost("verify/connect")]
        public async Task<IActionResult> VerifyConnectToken(VerifyConnectTokenViewModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
            {
                return BadRequest(new { Message = "Username is required." });
            }

            var user = await _userManager.FindByEmailAsync(model.Username);
            if (user == null)
            {
                return Unauthorized(new { Message = "User not found." });
            }
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
