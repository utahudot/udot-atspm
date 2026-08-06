#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages.Identity/IdentityServiceLogMessages.cs
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
    /// Provides strongly-typed high-performance logging methods for administrative user account operations and custom geography linkages.
    /// </summary>
    public partial class IdentityServiceLogMessages(ILogger logger)
    {
        private readonly ILogger _logger = logger;

        /// <summary>
        /// Logs when administrative user registration begins.
        /// </summary>
        /// <param name="email">The email of the user profile being registered.</param>
        [LoggerMessage(EventId = 1141, EventName = "Administrative Registration Initiated", Level = LogLevel.Information, Message = "Initiating administrative user creation for email: '{Email}'")]
        public partial void UserCreationInitiated(string email);

        /// <summary>
        /// Logs a successful administrative user registration.
        /// </summary>
        /// <param name="email">The email of the registered user.</param>
        /// <param name="userId">The newly generated identifier for the user account.</param>
        [LoggerMessage(EventId = 1142, EventName = "Administrative Registration Successful", Level = LogLevel.Information, Message = "Successfully created administrative user account: '{Email}' (ID: {UserId})")]
        public partial void UserCreatedSuccessfully(string email, string userId);

        /// <summary>
        /// Logs a failure during administrative user registration.
        /// </summary>
        /// <param name="email">The target email address.</param>
        /// <param name="error">The validation or exception description.</param>
        [LoggerMessage(EventId = 1143, EventName = "Administrative Registration Failed", Level = LogLevel.Error, Message = "Failed to create identity user '{Email}'. Errors: {Error}")]
        public partial void UserCreationFailed(string email, string error);

        /// <summary>
        /// Logs when a single user profile is requested.
        /// </summary>
        /// <param name="userId">The requested user account ID.</param>
        [LoggerMessage(EventId = 1144, EventName = "User Profile Queried", Level = LogLevel.Debug, Message = "Querying user details, roles, and custom geography profiles for user ID: '{UserId}'")]
        public partial void UserRetrievalById(string userId);

        /// <summary>
        /// Logs the initiation of a bulk user query.
        /// </summary>
        [LoggerMessage(EventId = 1145, EventName = "Bulk Users Query Initiated", Level = LogLevel.Information, Message = "Querying all registered users, in bulk, optimizing role and geographic profiles with optimized SQL joins.")]
        public partial void UsersBulkQueryInitiated();

        /// <summary>
        /// Logs the completion of a bulk user query.
        /// </summary>
        /// <param name="count">The total count of returned users.</param>
        [LoggerMessage(EventId = 1146, EventName = "Bulk Users Query Completed", Level = LogLevel.Information, Message = "Eager-loaded and mapped {Count} registered users successfully.")]
        public partial void UsersBulkQueryCompleted(int count);

        /// <summary>
        /// Logs the initiation of a user account modification.
        /// </summary>
        /// <param name="email">The target email account.</param>
        /// <param name="userId">The target user identifier.</param>
        [LoggerMessage(EventId = 1147, EventName = "User Account Modification Initiated", Level = LogLevel.Information, Message = "Updating user account fields, roles, and geographic links for: '{Email}' (ID: {UserId})")]
        public partial void UserUpdateInitiated(string email, string userId);

        /// <summary>
        /// Logs a completed user account modification.
        /// </summary>
        /// <param name="userId">The modified user identifier.</param>
        [LoggerMessage(EventId = 1148, EventName = "User Account Modification Completed", Level = LogLevel.Information, Message = "Successfully updated details and synchronized geographic mappings for user ID: '{UserId}'")]
        public partial void UserUpdateCompleted(string userId);

        /// <summary>
        /// Logs when a user account deletion is requested.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to delete.</param>
        [LoggerMessage(EventId = 1149, EventName = "User Deletion Initiated", Level = LogLevel.Information, Message = "Requesting user account and geographic linkage deletion for user ID: '{UserId}'")]
        public partial void UserDeletionInitiated(string userId);

        /// <summary>
        /// Logs when a user account is deleted successfully.
        /// </summary>
        /// <param name="userId">The unique identifier of the deleted user.</param>
        [LoggerMessage(EventId = 1150, EventName = "User Deletion Completed", Level = LogLevel.Information, Message = "Successfully deleted user profile, roles, and configuration geography links for user ID: '{UserId}'")]
        public partial void UserDeletionCompleted(string userId);
    }
}
