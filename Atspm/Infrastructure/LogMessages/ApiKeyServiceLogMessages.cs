#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/ApiKeyServiceLogMessages.cs
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
    /// Provides strongly-typed logging methods for the API key service operations.
    /// </summary>
    public partial class ApiKeyServiceLogMessages(ILogger logger)
    {
        private readonly ILogger _logger = logger;

        /// <summary>
        /// Logs the initiation of an API key creation.
        /// </summary>
        [LoggerMessage(EventId = 1101, EventName = "API Key Creation Initiated", Level = LogLevel.Information, Message = "Initiating API Key '{Name}' creation for target user '{UserId}' (requested by '{RequesterId}').")]
        public partial void KeyCreationInitiated(string name, string userId, string requesterId);

        /// <summary>
        /// Logs the successful creation of an API key.
        /// </summary>
        [LoggerMessage(EventId = 1102, EventName = "API Key Created", Level = LogLevel.Information, Message = "Successfully created API Key '{Name}' (ID: {Id}) for user '{UserId}'.")]
        public partial void KeyCreatedSuccessfully(string name, int id, string userId);

        /// <summary>
        /// Logs a security warning when a user attempts to delegate a claim they don't possess.
        /// </summary>
        [LoggerMessage(EventId = 1103, EventName = "Unauthorized Permission Delegation", Level = LogLevel.Warning, Message = "User '{RequesterId}' attempted to delegate permission '{Permission}' to API Key '{Name}' but does not possess it.")]
        public partial void UnauthorizedPermissionDelegated(string requesterId, string permission, string name);

        /// <summary>
        /// Logs when active API keys are retrieved for a user.
        /// </summary>
        [LoggerMessage(EventId = 1104, EventName = "User API Keys Retrieved", Level = LogLevel.Debug, Message = "Retrieving active API keys for user '{UserId}'.")]
        public partial void UserKeysRetrieved(string userId);

        /// <summary>
        /// Logs when an administrator retrieves all system API keys.
        /// </summary>
        [LoggerMessage(EventId = 1105, EventName = "System API Keys Retrieved", Level = LogLevel.Information, Message = "Administrator '{AdminId}' is retrieving all active system API keys.")]
        public partial void SystemKeysRetrieved(string adminId);

        /// <summary>
        /// Logs when a request is made to revoke an API key.
        /// </summary>
        [LoggerMessage(EventId = 1106, EventName = "API Key Revocation Requested", Level = LogLevel.Information, Message = "User '{RequesterId}' requested revocation of API Key ID {Id}.")]
        public partial void KeyRevocationRequested(string requesterId, int id);

        /// <summary>
        /// Logs when an API key is successfully revoked.
        /// </summary>
        [LoggerMessage(EventId = 1107, EventName = "API Key Revoked", Level = LogLevel.Information, Message = "API Key ID {Id} was successfully revoked.")]
        public partial void KeyRevokedSuccessfully(int id);

        /// <summary>
        /// Logs when an API key revocation request is made on an already revoked key.
        /// </summary>
        [LoggerMessage(EventId = 1108, EventName = "API Key Already Revoked", Level = LogLevel.Debug, Message = "API Key ID {Id} was already revoked.")]
        public partial void KeyAlreadyRevoked(int id);
    }
}
