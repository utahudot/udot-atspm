#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.Identity/AuthenticationService.cs
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

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.LogMessages.Identity;
using Utah.Udot.Atspm.Services.Identity;
using Utah.Udot.Atspm.Services.Identity.Dto;

namespace Utah.Udot.Atspm.Infrastructure.Services.Identity
{
    /// <inheritdoc cref="IAuthenticationService"/>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IOptions<IdentityConfiguration> _identityOptions;
        private readonly AuthenticationServiceLogMessages _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="signInManager">The ASP.NET Core Identity sign-in manager.</param>
        /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
        /// <param name="roleManager">The ASP.NET Core Identity role manager.</param>
        /// <param name="emailService">The infrastructure email delivery service.</param>
        /// <param name="configuration">The application configuration provider.</param>
        /// <param name="identityOptions">The specialized identity options binding.</param>
        /// <param name="logger">The logger instance.</param>
        public AuthenticationService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            IConfiguration configuration,
            IOptions<IdentityConfiguration> identityOptions,
            ILogger<AuthenticationService> logger)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _identityOptions = identityOptions ?? throw new ArgumentNullException(nameof(identityOptions));
            _log = new AuthenticationServiceLogMessages(logger ?? throw new ArgumentNullException(nameof(logger)));
        }

        /// <inheritdoc/>
        public async Task<AuthenticationResponseDto> LoginAsync(LoginRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _log.LoginAttempt(request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
            if (user == null)
            {
                _log.LoginFailure(request.Email, "Incorrect username or password");
                throw new UnauthorizedAccessException("Incorrect username or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                _log.LoginFailure(request.Email, "Incorrect username or password");
                throw new UnauthorizedAccessException("Incorrect username or password");
            }

            var token = await GenerateJwtTokenInternalAsync(user).ConfigureAwait(false);
            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            var permissions = await GetPermissionsForUserInternalAsync(user, roles).ConfigureAwait(false);

            _log.LoginSuccess(request.Email, user.Id);

            var expireDays = Convert.ToDouble(_configuration["Jwt:ExpireDays"] ?? "7");
            var expireInSeconds = (int)TimeSpan.FromDays(expireDays).TotalSeconds;

            return new AuthenticationResponseDto(
                user.Id,
                user.Email ?? string.Empty,
                token,
                Guid.NewGuid().ToString("N"),
                expireInSeconds,
                roles,
                permissions
            );
        }

        /// <inheritdoc/>
        public async Task LogoutAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            _log.LogoutRequested(userId);
            await _signInManager.SignOutAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TokenRefreshResponseDto> RefreshTokenAsync(TokenRefreshRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            throw new NotSupportedException("Refresh tokens are currently not configured in the host identity context. Please log in again.");
        }

        /// <inheritdoc/>
        public async Task InitiateForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _log.ForgotPasswordInitiated(request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
            if (user == null)
            {
                _log.ForgotPasswordFailed(request.Email, "User profile not found");
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
            var uriEncodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var callbackUrl = $"{_identityOptions.Value.Website}/change-password?username={user.UserName}&token={uriEncodedToken}";

            var mailMessage = new MailMessage(
                _identityOptions.Value.DefaultEmailAddress,
                request.Email,
                "Reset Password",
                $"<p>Please reset your password by clicking <a href=\"{callbackUrl}\">here</a>.</p>"
            )
            {
                IsBodyHtml = true
            };

            var emailSent = await _emailService.SendEmailAsync(mailMessage).ConfigureAwait(false);
            if (!emailSent)
            {
                _log.ForgotPasswordFailed(request.Email, "Email service delivery failed");
                throw new InvalidOperationException("Password reset email could not be sent because no email service is configured.");
            }
        }

        /// <inheritdoc/>
        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
            if (user == null)
            {
                _log.PasswordResetFailed(request.Email, "User profile not found");
                throw new KeyNotFoundException($"User with email '{request.Email}' was not found.");
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _log.PasswordResetFailed(request.Email, errors);
                throw new InvalidOperationException($"Password reset failed: {errors}");
            }

            _log.PasswordResetSuccess(request.Email);
        }

        /// <inheritdoc/>
        public async Task<bool> VerifyPasswordAsync(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{userId}' was not found.");
            }

            return await _userManager.CheckPasswordAsync(user, password).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                _log.PasswordChangeFailed(userId, "User profile not found");
                throw new KeyNotFoundException($"User with ID '{userId}' was not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _log.PasswordChangeFailed(userId, errors);
                throw new InvalidOperationException($"Failed to update password: {errors}");
            }

            _log.PasswordChangeSuccess(userId);
        }

        private async Task<string> GenerateJwtTokenInternalAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
            };

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            if (roles.Contains("Admin"))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            else
            {
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
                    if (role != null)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(role).ConfigureAwait(false);
                        foreach (var roleClaim in roleClaims)
                        {
                            claims.Add(new Claim(roleClaim.Type, roleClaim.Value));
                        }
                    }
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expireDays = Convert.ToDouble(_configuration["Jwt:ExpireDays"] ?? "7");
            var expires = DateTime.Now.AddDays(expireDays);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                null,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<IEnumerable<string>> GetPermissionsForUserInternalAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            var permissions = new List<string>();

            if (roles.Contains("Admin"))
            {
                permissions.Add("Admin");
            }
            else
            {
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
                    if (role != null)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(role).ConfigureAwait(false);
                        foreach (var claim in roleClaims)
                        {
                            permissions.Add(claim.Value);
                        }
                    }
                }
            }

            return permissions;
        }
    }
}
