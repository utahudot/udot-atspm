#region license
// Copyright 2025 Utah Departement of Transportation
// for IdentityApi - Identity.Controllers/AccountController.cs
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
using FluentFTP.Helpers;
using Identity.Business.Accounts;
using Identity.Models.Account;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Text;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.ATSPM.IdentityApi.Controllers;

namespace Identity.Controllers
{
    [ApiVersion("1.0")]
    public class AccountController : IdentityControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IEmailService emailService;
        private readonly IAccountService accountService;
        private readonly IConfiguration configuration;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAccountService accountService,
            IEmailService emailService,
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
            var redirectUri = Url.Action("OIDCLoginCallback", "Account");
            var properties = signInManager.ConfigureExternalAuthenticationProperties(OpenIdConnectDefaults.AuthenticationScheme, redirectUri);

            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        [HttpPost("OIDCLoginCallback")]
        [HttpGet("OIDCLoginCallback")]
        public async Task<IActionResult> OIDCLoginCallback()
        {
            var info = await signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                // Handle login failure (e.g., redirect to an error page)
                return BadRequest("External login information not available. Make sure you've authenticated with the external provider.");
            }

            var result = await accountService.HandleSsoRequest(info);

            if (result.Code == StatusCodes.Status200OK)
            {
                return Redirect($"{configuration["AtspmSite"]}/sso-login?token={result.Token}&claims={result.Claims.Join(",")}");
            }

            return Redirect($"{configuration["AtspmSite"]}/sso-login?error={result.Message}");
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
            var resetToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.ResetToken));
            var result = await userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (result != null && result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result?.Errors);
        }

        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromServices] IOptions<IdentityConfiguration> identityOptions, ForgotPasswordViewModel model)
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

            var callbackUrl = $"{identityOptions.Value.Website}/change-password?username=" + user.UserName + "&token=" + uriEncodedToken;

            //HACK: FIX THIS

            var message = new MailMessage(identityOptions.Value.DefaultEmailAddress, model.Email, "Reset Password", $"<p>Please reset your password by clicking <a href=\"{callbackUrl}\">here</a>.</p>");
            await emailService.SendEmailAsync(message);

            //await emailService.SendEmailAsync(
            //    model.Email,
            //    "Reset Password",
            //    $"<p>Please reset your password by clicking <a href=\"{callbackUrl}\">here</a>.</p>");

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
                var uriEncodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                return Ok(new { Token = uriEncodedToken, Username = user.UserName });
            }

            return BadRequest(new { Message = "Password provided doesn't match" });
        }
    }
}