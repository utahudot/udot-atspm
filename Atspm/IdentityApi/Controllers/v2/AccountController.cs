#region license
// Copyright 2026 Utah Departement of Transportation
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Services.Identity;
using Utah.Udot.Atspm.Services.Identity.Dto;

namespace Utah.Udot.ATSPM.IdentityApi.Controllers.v2
{
    /// <summary>
    /// Handles user account operations including authentication, password management, and Single Sign-On (SSO) integration.
    /// </summary>
    [ApiVersion("2.0")]
    [Produces("application/json")]
    public class AccountController : IdentityControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IFederatedAuthService _federatedAuthService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="federatedAuthService">The federated identity provider authentication service.</param>
        public AccountController(
            IAuthenticationService authenticationService,
            IFederatedAuthService federatedAuthService)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _federatedAuthService = federatedAuthService ?? throw new ArgumentNullException(nameof(federatedAuthService));
        }

        /// <summary>
        /// Authenticates a user with a local username and password, returning a JWT token upon success.
        /// </summary>
        /// <param name="model">The credential model containing user login details.</param>
        /// <returns>An authentication token and associated claims on success.</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthenticationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (model == null)
            {
                return BadRequest("Login credentials cannot be null.");
            }

            var result = await _authenticationService.LoginAsync(model);
            if (result != null)
            {
                return Ok(result);
            }

            return Unauthorized("Invalid login credentials.");
        }

        /// <summary>
        /// Revokes the current active session, invalidating the authorization token.
        /// </summary>
        /// <returns>A confirmation of successful sign-out.</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _authenticationService.LogoutAsync(userId);
            return Ok();
        }

        /// <summary>
        /// Initiates a secure password reset request, sending a verification link to the specified email address.
        /// </summary>
        /// <param name="model">The password reset request model containing the email address.</param>
        /// <returns>An empty result indicating the password reset flow was triggered.</returns>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            if (model == null)
            {
                return BadRequest("Forgot password request cannot be null.");
            }

            await _authenticationService.InitiateForgotPasswordAsync(model);
            return Ok();
        }

        /// <summary>
        /// Completes a password reset flow, applying the new password using the verified reset token.
        /// </summary>
        /// <param name="model">The password reset confirmation details.</param>
        /// <returns>An action result indicating success or a bad request if validation fails.</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto model)
        {
            if (model == null)
            {
                return BadRequest("Reset password request cannot be null.");
            }

            await _authenticationService.ResetPasswordAsync(model);
            return Ok();
        }

        /// <summary>
        /// Prepares security metadata for launching an external OIDC Single Sign-On (SSO) challenge.
        /// </summary>
        /// <param name="providerName">The name of the external OIDC identity provider.</param>
        /// <param name="redirectUri">The local callback URI where the identity provider should return.</param>
        /// <returns>The challenge properties required by the authentication handler.</returns>
        [HttpGet("external-challenge")]
        [ProducesResponseType(typeof(ChallengePropertiesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PrepareChallenge([FromQuery] string providerName, [FromQuery] string redirectUri)
        {
            if (string.IsNullOrWhiteSpace(providerName) || string.IsNullOrWhiteSpace(redirectUri))
            {
                return BadRequest("Provider name and redirect URI are required.");
            }

            var result = await _federatedAuthService.PrepareChallengeAsync(providerName, redirectUri);
            return Ok(result);
        }

        /// <summary>
        /// Handles Single Sign-On callbacks, returning local session tokens.
        /// </summary>
        /// <param name="providerName">The provider name issuing the identity.</param>
        /// <param name="externalInfo">The identity model representing the external claims.</param>
        /// <returns>A session token and profile structure.</returns>
        [HttpPost("external-callback")]
        [ProducesResponseType(typeof(FederatedLoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> HandleCallback([FromQuery] string providerName, [FromBody] ExternalIdentityDto externalInfo)
        {
            if (string.IsNullOrWhiteSpace(providerName) || externalInfo == null)
            {
                return BadRequest("Provider name and external identity metadata are required.");
            }

            var result = await _federatedAuthService.HandleCallbackAsync(providerName, externalInfo);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result.ErrorMessage);
        }

        /// <summary>
        /// Links an existing authenticated local user account to an external SSO provider login credential.
        /// </summary>
        /// <param name="userId">The ID of the local user account to link.</param>
        /// <param name="externalInfo">The identity model representing the external claims.</param>
        /// <returns>An action result indicating successful linkage.</returns>
        [HttpPost("link-external")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LinkExternal([FromQuery] string userId, [FromBody] ExternalIdentityDto externalInfo)
        {
            if (string.IsNullOrWhiteSpace(userId) || externalInfo == null)
            {
                return BadRequest("User ID and external identity metadata are required.");
            }

            await _federatedAuthService.LinkAccountAsync(userId, externalInfo);
            return Ok();
        }

        /// <summary>
        /// Retrieves the list of configured external OIDC Single Sign-On (SSO) identity provider names.
        /// </summary>
        /// <returns>A list of active identity provider names.</returns>
        [HttpGet("providers")]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<string>), StatusCodes.Status200OK)]
        public IActionResult GetConfiguredProviders()
        {
            var providers = _federatedAuthService.GetConfiguredProviders();
            return Ok(providers);
        }
    }
}
