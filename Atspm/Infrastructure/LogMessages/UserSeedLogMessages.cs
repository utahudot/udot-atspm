#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/UserSeedLogMessages.cs
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

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    /// <summary>
    /// Provides strongly-typed logging methods for the administrative user seeding process.
    /// </summary>
    public partial class UserSeedLogMessages(ILogger logger)
    {
        private readonly ILogger _logger = logger;

        /// <summary>
        /// Logs a warning when required environment variables for admin seeding are missing.
        /// </summary>
        [LoggerMessage(EventId = 211, EventName = "Missing Admin Credentials", Level = LogLevel.Warning, Message = "Admin seeding skipped: Missing ADMIN_EMAIL or ADMIN_PASSWORD environment variables.")]
        public partial void MissingCredentials();

        /// <summary>
        /// Logs that the admin user was not found and creation is starting.
        /// </summary>
        /// <param name="email">The email of the admin user being created.</param>
        [LoggerMessage(EventId = 212, EventName = "Admin User Not Found", Level = LogLevel.Information, Message = "Admin user {Email} not found. Creating...")]
        public partial void AdminNotFound(string email);

        /// <summary>
        /// Logs that a specific role was missing and is being created.
        /// </summary>
        /// <param name="role">The name of the role being created.</param>
        [LoggerMessage(EventId = 213, EventName = "Creating Missing Role", Level = LogLevel.Information, Message = "Role {Role} not found during user seeding, creating it now.")]
        public partial void CreatingRole(string role);

        /// <summary>
        /// Logs the successful creation and role assignment of the admin user.
        /// </summary>
        /// <param name="email">The email of the created user.</param>
        /// <param name="role">The role assigned to the user.</param>
        [LoggerMessage(EventId = 214, EventName = "Admin Seeding Success", Level = LogLevel.Information, Message = "Admin user {Email} created and assigned to {Role} successfully.")]
        public partial void SeedingSuccess(string email, string role);

        /// <summary>
        /// Logs a failure to assign a user to a role.
        /// </summary>
        /// <param name="role">The target role.</param>
        /// <param name="errors">The concatenated error descriptions from Identity.</param>
        [LoggerMessage(EventId = 215, EventName = "Role Assignment Error", Level = LogLevel.Error, Message = "User created but failed to assign to role {Role}: {Errors}")]
        public partial void RoleAssignmentError(string role, string errors);

        /// <summary>
        /// Logs a failure to create the user object.
        /// </summary>
        /// <param name="errors">The concatenated error descriptions from Identity.</param>
        [LoggerMessage(EventId = 216, EventName = "User Creation Error", Level = LogLevel.Error, Message = "Failed to create admin user: {Errors}")]
        public partial void UserCreationError(string errors);

        /// <summary>
        /// Logs that the admin user already exists, so seeding is skipped.
        /// </summary>
        /// <param name="email">The email of the existing user.</param>
        [LoggerMessage(EventId = 217, EventName = "Admin Already Exists", Level = LogLevel.Debug, Message = "Admin user {Email} already exists. Skipping user creation.")]
        public partial void UserAlreadyExists(string email);
    }
}
