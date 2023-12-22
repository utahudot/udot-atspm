using Identity.Business.Accounts;
using Identity.Business.EmailSender;
using Identity.Business.ScopeService;
using Identity.Models.Account;
using IdentityModel.Client;
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
        private readonly IEmailService _emailService;
        private readonly IAccountService _accountService;
        private readonly IScopeService _scopeService;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAccountService accountService,
            IEmailService emailService,
            IScopeService scopeService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountService = accountService;
            _emailService = emailService;
            _scopeService = scopeService;
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

            AccountResult result = await _accountService.CreateUser(user, model.Password);
            if (result.Code == StatusCodes.Status400BadRequest)
            {
                return BadRequest(result);
            }
            if (result.Code == StatusCodes.Status500InternalServerError)
            {
                return new ObjectResult(result) { StatusCode = result.Code };
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
                return BadRequest(ModelState);
            }

            var authenticationResult = await _accountService.Login(model.Email, model.Password, model.RememberMe);

            if (authenticationResult.Code == StatusCodes.Status200OK)
            {
                // Assuming _scopeService is injected and provides access to the database for fetching allowed scopes
                var allowedScopes = _scopeService.GetScopesForClient(authenticationResult.ClientId);

                // Request the access token from the Identity Server
                var tokenClient = new TokenClient(
                    Configuration["IdentityServer:TokenEndpoint"],
                    authenticationResult.ClientId,
                    Configuration["IdentityServer:ClientSecret"]);

                var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(
                    model.Email,
                    model.Password,
                    allowedScopes);

                if (tokenResponse.IsError)
                {
                    // Handle error, e.g., return an error response to the client
                    return BadRequest($"Authentication succeeded, but token request failed. Error: {tokenResponse.Error}");
                }

                // Access token obtained successfully, you can use it or return it to the client
                var accessToken = tokenResponse.AccessToken;

                // Other logic, e.g., return user information along with the access token
                return Ok(new { AccessToken = accessToken, UserInfo = /* user information */ });
            }

            return BadRequest(authenticationResult);
        }

        [HttpPost("external-login")]
        public IActionResult ExternalLogin(LoginViewModel model)
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", "http://localhost:44357/api/account/google-callback");
            return Challenge(properties, "Google");

        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // Handle external login failure
                return BadRequest();
            }

            // Sign in the user using external login information
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                // Redirect to the desired URL after successful login
                return Redirect("http://localhost:3000/callback");
            }
            else
            {
                // Handle the case where the user does not have an account yet
                // You may want to redirect the user to a registration page
                return BadRequest("User does not have an account");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Successfully logged out." });
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
            if (user == null)
            {
                return Ok();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //var callbackUrl = Url.Action(
            //    "ResetPassword", // Action method to reset password in your web application
            //    "Account",
            //    new { email = user.Email, token },
            //    protocol: HttpContext.Request.Scheme);
            var callbackUrl = "http://localhost:3000/changepassword?username=" + user.UserName + "&code=" + token;

            await _emailService.SendEmailAsync(
                model.Email,
                "Reset Password",
                $"Please reset your password by clicking here: {callbackUrl}");

            // You can return a success message or any other relevant information
            return Ok();
        }
    }

}