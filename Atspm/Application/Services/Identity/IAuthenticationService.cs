#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services.Identity/IAuthenticationService.cs
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

using Utah.Udot.Atspm.Services.Identity.Dto;

namespace Utah.Udot.Atspm.Services.Identity
{
    /// <summary>
    /// Service contract for handling user session lifecycles, JWT token generations, 
    /// credentials validation, and self-service password cycles.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates user credentials and returns a JSON Web Token (JWT) along with refresh token and claims.
        /// </summary>
        /// <param name="request">The login request credentials.</param>
        /// <returns>A task representing the asynchronous operation, containing the authentication response details.</returns>
        Task<AuthenticationResponseDto> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// Revokes the current session and token context for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to log out.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogoutAsync(string userId);

        /// <summary>
        /// Generates a new access token using a cryptographically secure refresh token.
        /// </summary>
        /// <param name="request">The token refresh credentials.</param>
        /// <returns>A task representing the asynchronous operation, containing the new token details.</returns>
        Task<TokenRefreshResponseDto> RefreshTokenAsync(TokenRefreshRequestDto request);

        /// <summary>
        /// Initiates a self-service password reset workflow, generating and sending a token via an configured channel.
        /// </summary>
        /// <param name="request">The forgot password request details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InitiateForgotPasswordAsync(ForgotPasswordRequestDto request);

        /// <summary>
        /// Resets a user's password using a verified email reset token.
        /// </summary>
        /// <param name="request">The password reset validation details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ResetPasswordAsync(ResetPasswordRequestDto request);

        /// <summary>
        /// Performs self-service validation of a user's password.
        /// Useful for administrative or profile-level gate checks before updating fields.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="password">The plaintext password to verify.</param>
        /// <returns>A task representing the asynchronous operation, containing true if verified, false otherwise.</returns>
        Task<bool> VerifyPasswordAsync(string userId, string password);

        /// <summary>
        /// Updates an authenticated user's password.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="request">The old and new password details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
    }
}
