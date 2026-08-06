#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.Identity/FederatedAuthService.cs
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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Services.Identity;
using Utah.Udot.Atspm.Services.Identity.Dto;
using Utah.Udot.Atspm.Infrastructure.LogMessages.Identity;

namespace Utah.Udot.Atspm.Infrastructure.Services.Identity
{
    /// <inheritdoc cref="IFederatedAuthService"/>
    public class FederatedAuthService : IFederatedAuthService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly FederatedAuthServiceLogMessages _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="FederatedAuthService"/> class.
        /// </summary>
        /// <param name="signInManager">The ASP.NET Core Identity sign-in manager.</param>
        /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
        /// <param name="roleManager">The ASP.NET Core Identity role manager.</param>
        /// <param name="configuration">The application configuration provider.</param>
        /// <param name="logger">The logger instance.</param>
        public FederatedAuthService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<FederatedAuthService> logger)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _log = new FederatedAuthServiceLogMessages(logger ?? throw new ArgumentNullException(nameof(logger)));
        }

        /// <inheritdoc/>
        public async Task<ChallengePropertiesDto> PrepareChallengeAsync(string providerName, string redirectUri)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("Provider name cannot be null or empty.", nameof(providerName));
            }

            _log.ChallengePrepared(providerName, redirectUri);

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(providerName, redirectUri);
            var itemsDict = properties.Items.ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? string.Empty);

            return await Task.FromResult(new ChallengePropertiesDto(
                providerName,
                redirectUri,
                itemsDict
            )).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<FederatedLoginResponseDto> HandleCallbackAsync(string providerName, ExternalIdentityDto externalInfo)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("Provider name cannot be null or empty.", nameof(providerName));
            }
            if (externalInfo == null)
            {
                throw new ArgumentNullException(nameof(externalInfo));
            }

            externalInfo.UserClaims.TryGetValue(ClaimTypes.Email, out var email);
            if (string.IsNullOrWhiteSpace(email))
            {
                externalInfo.UserClaims.TryGetValue("email", out email);
            }

            externalInfo.UserClaims.TryGetValue(ClaimTypes.GivenName, out var firstName);
            if (string.IsNullOrWhiteSpace(firstName))
            {
                externalInfo.UserClaims.TryGetValue("givenname", out firstName);
            }

            externalInfo.UserClaims.TryGetValue(ClaimTypes.Surname, out var lastName);
            if (string.IsNullOrWhiteSpace(lastName))
            {
                externalInfo.UserClaims.TryGetValue("surname", out lastName);
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                _log.SSOCallbackFailed(providerName, "Required claims (email, givenname, surname) missing from external metadata.");
                return new FederatedLoginResponseDto(false, "Required user claims not returned by the SSO provider.", string.Empty, Enumerable.Empty<string>());
            }

            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            var loginInfo = await _userManager.FindByLoginAsync(providerName, externalInfo.ProviderKey).ConfigureAwait(false);

            if (user == null && loginInfo == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Agency = string.Empty,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user).ConfigureAwait(false);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    _log.SSOCallbackFailed(providerName, $"Failed to auto-provision user account: {errors}");
                    return new FederatedLoginResponseDto(false, $"Failed to create user account: {errors}", string.Empty, Enumerable.Empty<string>());
                }

                var linkResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(providerName, externalInfo.ProviderKey, providerName)).ConfigureAwait(false);
                if (!linkResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user).ConfigureAwait(false);
                    _log.SSOCallbackFailed(providerName, "Failed to link external SSO credentials to auto-provisioned account.");
                    return new FederatedLoginResponseDto(false, "Failed to link SSO login credentials to new user account.", string.Empty, Enumerable.Empty<string>());
                }

                _log.NewSSOUserCreated(providerName, email, user.Id);
            }
            else if (user != null && loginInfo == null)
            {
                var linkResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(providerName, externalInfo.ProviderKey, providerName)).ConfigureAwait(false);
                if (!linkResult.Succeeded)
                {
                    _log.SSOCallbackFailed(providerName, "Failed to link external SSO credentials to existing account.");
                    return new FederatedLoginResponseDto(false, "Failed to link SSO login credentials to existing user account.", string.Empty, Enumerable.Empty<string>());
                }

                _log.AccountLinked(providerName, user.Id);
            }

            await _signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(false);

            var token = await GenerateJwtTokenInternalAsync(user).ConfigureAwait(false);
            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            var permissions = await GetPermissionsForUserInternalAsync(user, roles).ConfigureAwait(false);

            _log.SSOCallbackProcessed(providerName, email);

            return new FederatedLoginResponseDto(true, string.Empty, token, permissions);
        }

        /// <inheritdoc/>
        public async Task LinkAccountAsync(string userId, ExternalIdentityDto externalInfo)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }
            if (externalInfo == null)
            {
                throw new ArgumentNullException(nameof(externalInfo));
            }

            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{userId}' was not found.");
            }

            var linkedUser = await _userManager.FindByLoginAsync(externalInfo.ProviderName, externalInfo.ProviderKey).ConfigureAwait(false);
            if (linkedUser != null)
            {
                throw new InvalidOperationException("This external account is already linked to another user.");
            }

            var result = await _userManager.AddLoginAsync(user, new UserLoginInfo(externalInfo.ProviderName, externalInfo.ProviderKey, externalInfo.ProviderName)).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _log.SSOCallbackFailed(externalInfo.ProviderName, $"Failed to link SSO account to user {userId}: {errors}");
                throw new InvalidOperationException($"Failed to link external SSO account: {errors}");
            }

            _log.AccountLinked(externalInfo.ProviderName, userId);
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
