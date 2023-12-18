using Identity.Models.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerifyTokenController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyTokenController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyToken(VerifyTokenViewModel model)
        {
            if (string.IsNullOrEmpty(model.Token))
            {
                return BadRequest(new { message = "Token is required." });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Verify the reset token
            var result = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, UserManager<ApplicationUser>.ResetPasswordTokenPurpose, token);

            if (result)
            {
                return Ok(new { message = "OK" });
            }
            else
            {
                return Unauthorized(new { message = "Unauthorized" });
            }
        }
    }
}
