using Identity.Business.Accounts;
using Identity.Business.EmailSender;
using Identity.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly EmailService emailService;
        private readonly IAccountService accountService;
        private readonly IConfiguration configuration;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAccountService accountService,
            EmailService emailService,
            IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.accountService = accountService;
            this.emailService = emailService;
            this.configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid || AreValuesNull(model))
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Agency = model.Agency,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            AccountResult result = await accountService.CreateUser(user, model.Password);
            if (result.Code == StatusCodes.Status400BadRequest)
            {
                return BadRequest(result);
            }
            if (result.Code == StatusCodes.Status500InternalServerError)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
            //check agencyExists
            // if no, then create user with admin roles
            // if yes, then just create user with no roles
            // then log in the user as well and send back the credentials for logged in
            //if error in any processes above can just send back the result since identity service also handles the errors or success message

            return Ok(result);
        }

        private bool AreValuesNull(RegisterViewModel model)
        {
            return string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Error = "Not a valid Request" });
            }

            var authenticationResult = await accountService.Login(model.Email, model.Password, model.RememberMe);

            if (authenticationResult.Code == StatusCodes.Status200OK)
            {
                // Assuming the authenticationResult includes the generated JWT token
                var token = authenticationResult.Token;

                if (string.IsNullOrEmpty(token))
                {
                    authenticationResult.Message = "Error generating token.";
                    return StatusCode(StatusCodes.Status500InternalServerError, authenticationResult);
                }

                return Ok(authenticationResult);
            }

            return BadRequest(authenticationResult);
        }

        [HttpGet("external-login")]
        public IActionResult ExternalLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("OIDCLoginCallback", "Account") };
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }



        [HttpGet("OIDCLoginCallback")]
        public async Task<IActionResult> OIDCLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                // Handle the error received from the external provider
                return RedirectToAction("Login", new { ErrorMessage = remoteError });
            }

            // Retrieve the login information from the external provider
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // No information was returned
                return RedirectToAction("Login", new { ErrorMessage = "Error loading external login information." });
            }

            // Attempt to sign in the user with the external login info
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                // Sign-in success, redirect to the return URL or a default page
                return LocalRedirect(returnUrl ?? "/");
            }
            else if (result.IsLockedOut)
            {
                // Handle if the user is locked out
                return RedirectToAction("Lockout");
            }
            else
            {
                // User does not have a local account, you may need to create one
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var userName = email;

                // You can extract more information from the info object as needed
                var user = new ApplicationUser { UserName = userName, Email = email };

                var identityResult = await userManager.CreateAsync(user);
                if (identityResult.Succeeded)
                {
                    identityResult = await userManager.AddLoginAsync(user, info);
                    if (identityResult.Succeeded)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl ?? "/");
                    }
                }

                // If there were any issues with account creation, handle them appropriately
                return RedirectToAction("Login", new { ErrorMessage = "Could not create user account." });
            }
        }


        [Authorize]
        [HttpPost("link-external-login")]
        public async Task<IActionResult> LinkExternalLogin()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            // Retrieve the external login info from the temporary sign-in that occurs during external authentication
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return BadRequest("External login information not available. Make sure you've authenticated with the external provider.");
            }

            // Check if the external login is already linked
            var result = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (result != null)
            {
                return BadRequest("This external account is already linked to another user.");
            }

            // Link the external login to the user's account
            var linkResult = await userManager.AddLoginAsync(user, info);
            if (linkResult.Succeeded)
            {
                // Optionally, sign the user in with the new external login
                await signInManager.SignInAsync(user, isPersistent: false);
                return Ok("The external account has been linked successfully.");
            }

            return BadRequest("Failed to link the external account.");
        }



        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok(new { Message = "Successfully logged out." });
        }

        [Authorize]
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Error = "Not a valid Request" });
            }

            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized("User not found");
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

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

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Ok();
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var uriEncodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            //var callbackUrl = Url.Action(
            //    "ResetPassword", // Action method to reset password in your web application
            //    "Account",
            //    new { email = user.Email, token },
            //    protocol: HttpContext.Request.Scheme);
            var callbackUrl = $"{configuration["AtspmSite"]}/changePassword?username=" + user.UserName + "&token=" + uriEncodedToken;

            await emailService.SendEmailAsync(
                model.Email,
                "Reset Password",
                $"<p>Please reset your password by clicking <a href=\"{callbackUrl}\">here</a>.</p>");

            // You can return a success message or any other relevant information
            return Ok();
        }

        [Authorize]
        [HttpPost("verifyUserPasswordReset")]
        public async Task<IActionResult> VerifyUserPasswordReset(VerifyUserPasswordResetViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            var isUserVerified = await userManager.CheckPasswordAsync(user, model.Password);

            if (isUserVerified)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                return Ok(new { token, Username = user.UserName });
            }

            return BadRequest(new { Message = "Password provided doesn't match" });
        }
    }
}