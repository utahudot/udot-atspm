#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services.Identity.Dto/AuthenticationDtos.cs
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

using System.Collections.Generic;

namespace Utah.Udot.Atspm.Services.Identity.Dto
{
    /// <summary>
    /// Data transfer object representing the login credentials of a user.
    /// </summary>
    public record LoginRequestDto(
        string Email,
        string Password,
        bool RememberMe
    );

    /// <summary>
    /// Data transfer object containing the result of a successful user authentication.
    /// </summary>
    public record AuthenticationResponseDto(
        string UserId,
        string Email,
        string AccessToken,
        string RefreshToken,
        int ExpiresInSeconds,
        IEnumerable<string> Roles,
        IEnumerable<string> Permissions
    );

    /// <summary>
    /// Data transfer object containing the refresh token used to request a new access token.
    /// </summary>
    public record TokenRefreshRequestDto(
        string RefreshToken
    );

    /// <summary>
    /// Data transfer object containing the newly issued access and refresh tokens.
    /// </summary>
    public record TokenRefreshResponseDto(
        string AccessToken,
        string RefreshToken,
        int ExpiresInSeconds
    );

    /// <summary>
    /// Data transfer object representing a request to initiate a password reset.
    /// </summary>
    public record ForgotPasswordRequestDto(
        string Email
    );

    /// <summary>
    /// Data transfer object representing the credentials needed to reset a user's password using a verified token.
    /// </summary>
    public record ResetPasswordRequestDto(
        string Email,
        string Token,
        string NewPassword
    );

    /// <summary>
    /// Data transfer object representing a request to change an authenticated user's password.
    /// </summary>
    public record ChangePasswordRequestDto(
        string OldPassword,
        string NewPassword
    );
}
