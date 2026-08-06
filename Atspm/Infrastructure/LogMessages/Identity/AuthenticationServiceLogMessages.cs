#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages.Identity/AuthenticationServiceLogMessages.cs
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

using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages.Identity
{
    /// <summary>
    /// Provides strongly-typed high-performance logging methods for local authentication and credential operations.
    /// </summary>
    public partial class AuthenticationServiceLogMessages(ILogger logger)
    {
        private readonly ILogger _logger = logger;

        /// <summary>
        /// Logs a user login attempt.
        /// </summary>
        /// <param name="email">The email of the user attempting login.</param>
        [LoggerMessage(EventId = 1121, EventName = "Login Attempt", Level = LogLevel.Information, Message = "Initiating credentials validation and session generation for: '{Email}'")]
        public partial void LoginAttempt(string email);

        /// <summary>
        /// Logs a successful user login.
        /// </summary>
        /// <param name="email">The email of the authenticated user.</param>
        /// <param name="userId">The unique identifier of the authenticated user.</param>
        [LoggerMessage(EventId = 1122, EventName = "Login Successful", Level = LogLevel.Information, Message = "User '{Email}' (ID: {UserId}) authenticated successfully. JWT and access context generated.")]
        public partial void LoginSuccess(string email, string userId);

        /// <summary>
        /// Logs a failed user login attempt.
        /// </summary>
        /// <param name="email">The email of the user attempting login.</param>
        /// <param name="error">The validation error description.</param>
        [LoggerMessage(EventId = 1123, EventName = "Login Failed", Level = LogLevel.Warning, Message = "Authentication failed for user '{Email}'. Reason: {Error}")]
        public partial void LoginFailure(string email, string error);

        /// <summary>
        /// Logs a user logout request.
        /// </summary>
        /// <param name="userId">The unique identifier of the logging out user.</param>
        [LoggerMessage(EventId = 1124, EventName = "Logout Requested", Level = LogLevel.Information, Message = "Session logout requested and completed for user ID: '{UserId}'")]
        public partial void LogoutRequested(string userId);

        /// <summary>
        /// Logs the initiation of a password reset dispatch.
        /// </summary>
        /// <param name="email">The email address requesting the reset link.</param>
        [LoggerMessage(EventId = 1125, EventName = "Forgot Password Initiated", Level = LogLevel.Information, Message = "Initiating forgot password token generation and dispatch for: '{Email}'")]
        public partial void ForgotPasswordInitiated(string email);

        /// <summary>
        /// Logs a failure in password reset link dispatch.
        /// </summary>
        /// <param name="email">The target email address.</param>
        /// <param name="error">The exception details.</param>
        [LoggerMessage(EventId = 1126, EventName = "Forgot Password Failed", Level = LogLevel.Error, Message = "Failed to completely process forgot password cycle for '{Email}'. Error: {Error}")]
        public partial void ForgotPasswordFailed(string email, string error);

        /// <summary>
        /// Logs a successful password reset validation.
        /// </summary>
        /// <param name="email">The target email address.</param>
        [LoggerMessage(EventId = 1127, EventName = "Password Reset Successful", Level = LogLevel.Information, Message = "Password reset credentials successfully validated and synchronized for user: '{Email}'")]
        public partial void PasswordResetSuccess(string email);

        /// <summary>
        /// Logs a failed password reset validation.
        /// </summary>
        /// <param name="email">The target email address.</param>
        /// <param name="error">The failure description.</param>
        [LoggerMessage(EventId = 1128, EventName = "Password Reset Failed", Level = LogLevel.Warning, Message = "Password reset validation failed for user '{Email}'. Errors: {Error}")]
        public partial void PasswordResetFailed(string email, string error);

        /// <summary>
        /// Logs a successful credentials change.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        [LoggerMessage(EventId = 1129, EventName = "Credentials Change Successful", Level = LogLevel.Information, Message = "Security credentials updated successfully for user ID: '{UserId}'")]
        public partial void PasswordChangeSuccess(string userId);

        /// <summary>
        /// Logs a failed credentials change.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="error">The failure description.</param>
        [LoggerMessage(EventId = 1130, EventName = "Credentials Change Failed", Level = LogLevel.Warning, Message = "Security credentials update failed for user ID '{UserId}'. Errors: {Error}")]
        public partial void PasswordChangeFailed(string userId, string error);
    }
}
