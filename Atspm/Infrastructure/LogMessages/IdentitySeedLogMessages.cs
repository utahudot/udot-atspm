#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/HostedServiceLogMessages.cs
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
    /// Provides strongly-typed logging methods for the Identity roles and claims seeding process.
    /// </summary>
    public partial class IdentitySeedLogMessages(ILogger logger)
    {
        private readonly ILogger _logger = logger;

        /// <summary>
        /// Logs that the identity roles already exist and the seeding process is being skipped.
        /// </summary>
        [LoggerMessage(EventId = 221, EventName = "Identity Roles Exist", Level = LogLevel.Information, Message = "Identity roles already exist. Skipping claims seeding.")]
        public partial void RolesAlreadyExist();

        /// <summary>
        /// Logs the successful creation of a new identity role.
        /// </summary>
        /// <param name="role">The name of the role created.</param>
        [LoggerMessage(EventId = 222, EventName = "Role Created", Level = LogLevel.Information, Message = "Created role: {Role}")]
        public partial void RoleCreated(string role);

        /// <summary>
        /// Logs a failure to create a specific identity role.
        /// </summary>
        /// <param name="role">The name of the role that failed to create.</param>
        /// <param name="errors">The error descriptions returned by the RoleManager.</param>
        [LoggerMessage(EventId = 223, EventName = "Role Creation Error", Level = LogLevel.Error, Message = "Could not create role {Role}: {Errors}")]
        public partial void RoleCreationError(string role, string errors);

        /// <summary>
        /// Logs that a specific permission (claim) has been added to a role.
        /// </summary>
        /// <param name="permission">The permission string being added.</param>
        /// <param name="role">The role receiving the permission.</param>
        [LoggerMessage(EventId = 224, EventName = "Permission Added", Level = LogLevel.Debug, Message = "Added permission {Permission} to role {Role}")]
        public partial void PermissionAdded(string permission, string role);
    }
}
