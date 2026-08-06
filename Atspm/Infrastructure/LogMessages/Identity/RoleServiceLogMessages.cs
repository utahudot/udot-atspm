#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages.Identity/RoleServiceLogMessages.cs
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
    /// Provides strongly-typed high-performance logging methods for security role and permission claim management.
    /// </summary>
    public partial class RoleServiceLogMessages(ILogger logger)
    {
        private readonly ILogger _logger = logger;

        /// <summary>
        /// Logs when a security role creation is initiated.
        /// </summary>
        /// <param name="roleName">The unique name of the role being created.</param>
        [LoggerMessage(EventId = 1161, EventName = "Role Creation Initiated", Level = LogLevel.Information, Message = "Creating security role: '{RoleName}'")]
        public partial void RoleCreationInitiated(string roleName);

        /// <summary>
        /// Logs when a security role is created successfully.
        /// </summary>
        /// <param name="roleName">The unique name of the created role.</param>
        [LoggerMessage(EventId = 1162, EventName = "Role Creation Successful", Level = LogLevel.Information, Message = "Successfully registered security role '{RoleName}' in the system.")]
        public partial void RoleCreatedSuccessfully(string roleName);

        /// <summary>
        /// Logs when a security role deletion is initiated.
        /// </summary>
        /// <param name="roleName">The unique name of the role being deleted.</param>
        [LoggerMessage(EventId = 1163, EventName = "Role Deletion Initiated", Level = LogLevel.Information, Message = "Deleting security role: '{RoleName}'")]
        public partial void RoleDeletionInitiated(string roleName);

        /// <summary>
        /// Logs when a security role is deleted successfully.
        /// </summary>
        /// <param name="roleName">The unique name of the deleted role.</param>
        [LoggerMessage(EventId = 1164, EventName = "Role Deletion Successful", Level = LogLevel.Information, Message = "Successfully deleted security role '{RoleName}' and unassigned all associated user/claim links.")]
        public partial void RoleDeletedSuccessfully(string roleName);

        /// <summary>
        /// Logs when syncing permission claims to a role begins.
        /// </summary>
        /// <param name="roleName">The unique name of the role.</param>
        [LoggerMessage(EventId = 1165, EventName = "Role Claims Sync Initiated", Level = LogLevel.Information, Message = "Synchronizing permission claims for role: '{RoleName}'")]
        public partial void RoleSyncClaimsInitiated(string roleName);

        /// <summary>
        /// Logs when syncing permission claims to a role completes.
        /// </summary>
        /// <param name="roleName">The unique name of the role.</param>
        /// <param name="added">The number of added permission claims.</param>
        /// <param name="removed">The number of removed permission claims.</param>
        [LoggerMessage(EventId = 1166, EventName = "Role Claims Sync Completed", Level = LogLevel.Information, Message = "Successfully updated claims on role '{RoleName}'. Permissions added: {Added}, removed: {Removed}.")]
        public partial void RoleSyncClaimsCompleted(string roleName, int added, int removed);

        /// <summary>
        /// Logs when all available system permissions are requested.
        /// </summary>
        [LoggerMessage(EventId = 1167, EventName = "System Permissions Queried", Level = LogLevel.Debug, Message = "Querying all possible functional permissions from System ClaimTypes metadata.")]
        public partial void SystemPermissionsQueried();
    }
}
