using Identity.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result != null && result.Succeeded)
            {
                // Optionally, you can sign the user in after successful registration.
                // await _signInManager.SignInAsync(user, isPersistent: false);

                return Ok();
            }

            return BadRequest(result?.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result != null && result.Succeeded)
            {
                // Build the return URL after successful token issuance
                var returnUrl = Url.Action("Index", "Home"); // adjust based on your needs

                // Redirect the user to IdentityServer for token issuance
                return Redirect($"[Your_IdentityServer_Endpoint]/connect/authorize?client_id=[Your_Client_Id]&response_type=code&redirect_uri={returnUrl}");

                // Note: The above URL is a simplification. In reality, you'd likely use an OIDC client library to help with creating this URL.

            }

            return Unauthorized();
        }

        //private string GenerateJwtToken(ApplicationUser user)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]); // Replace "Secret" with your own secret key
        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(new[]
        //        {
        //            new Claim(ClaimTypes.NameIdentifier, user.Id),
        //            new Claim(ClaimTypes.Email, user.Email),
        //            // Add other claims as needed (e.g., roles, custom claims, etc.)
        //        }),
        //        Expires = DateTime.UtcNow.AddMinutes(30), // Set token expiration time
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //    };

        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    return tokenHandler.WriteToken(token);
        //}

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost("changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result != null && result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result?.Errors);
        }

        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (model.Email == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // To prevent user enumeration attacks, return a generic error message
                // instead of providing information whether the user exists or the email is confirmed.
                return Ok("An email will be sent with the reset instructions.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Send the password reset token to the user's email for further steps.

            return Ok("An email will be sent with the reset instructions");
        }
    }

}